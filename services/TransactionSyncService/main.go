package main

import (
	"context"
	"encoding/json"
	"fmt"
	"os"
	"os/signal"
	"sync"
	"syscall"
	"time"

	"net/http"

	"github.com/confluentinc/confluent-kafka-go/v2/kafka"
	"github.com/jackc/pgx/v5"
	"github.com/jackc/pgx/v5/pgxpool"
	"github.com/prometheus/client_golang/prometheus"
	"github.com/prometheus/client_golang/prometheus/promhttp"
	"go.opentelemetry.io/otel"
	"go.opentelemetry.io/otel/exporters/otlp/otlptrace/otlptracehttp"
	"go.opentelemetry.io/otel/sdk/resource"
	sdktrace "go.opentelemetry.io/otel/sdk/trace"
	semconv "go.opentelemetry.io/otel/semconv/v1.17.0"
	"go.opentelemetry.io/otel/trace"
	"go.uber.org/zap"
)

type Transaction struct {
	ID                string    `json:"id"`
	SourceID          string    `json:"sourceId"`
	AccountNumber     string    `json:"accountNumber"`
	AccountHolderName string    `json:"accountHolderName"`
	Amount            float64   `json:"amount"`
	Currency          string    `json:"currency"`
	Description       string    `json:"description"`
	Merchant          string    `json:"merchant"`
	Location          string    `json:"location"`
	TransactionType   string    `json:"transactionType"`
	Category          string    `json:"category"`
	ReferenceNumber   string    `json:"referenceNumber"`
	TransactionTime   time.Time `json:"transactionTime"`
	ProcessedAt       time.Time `json:"processedAt"`
}

type SyncService struct {
	consumer     *kafka.Consumer
	db           *pgxpool.Pool
	logger       *zap.Logger
	tracer       trace.Tracer
	metrics      *Metrics
	shutdownChan chan os.Signal
	wg           sync.WaitGroup
}

type Metrics struct {
	messagesProcessed  prometheus.Counter
	messagesFailed     prometheus.Counter
	processingDuration prometheus.Histogram
	dbInsertDuration   prometheus.Histogram
	syncLag            prometheus.Gauge
}

func newMetrics() *Metrics {
	return &Metrics{
		messagesProcessed: prometheus.NewCounter(prometheus.CounterOpts{
			Name: "transaction_sync_messages_processed_total",
			Help: "Total number of messages processed from Kafka",
		}),
		messagesFailed: prometheus.NewCounter(prometheus.CounterOpts{
			Name: "transaction_sync_messages_failed_total",
			Help: "Total number of messages that failed to sync",
		}),
		processingDuration: prometheus.NewHistogram(prometheus.HistogramOpts{
			Name:    "transaction_sync_processing_duration_seconds",
			Help:    "Duration of processing transactions from Kafka",
			Buckets: prometheus.DefBuckets,
		}),
		dbInsertDuration: prometheus.NewHistogram(prometheus.HistogramOpts{
			Name:    "transaction_sync_db_insert_duration_seconds",
			Help:    "Duration of inserting transactions into database",
			Buckets: prometheus.DefBuckets,
		}),
		syncLag: prometheus.NewGauge(prometheus.GaugeOpts{
			Name: "transaction_sync_lag",
			Help: "Current sync lag in milliseconds",
		}),
	}
}

func (m *Metrics) Register() error {
	prometheus.MustRegister(m.messagesProcessed)
	prometheus.MustRegister(m.messagesFailed)
	prometheus.MustRegister(m.processingDuration)
	prometheus.MustRegister(m.dbInsertDuration)
	prometheus.MustRegister(m.syncLag)
	return nil
}

func initTracer(endpoint string, logger *zap.Logger) (trace.Tracer, func(), error) {
	exporter, err := otlptracehttp.New(context.Background(),
		otlptracehttp.WithEndpoint(endpoint),
	)
	if err != nil {
		logger.Error("failed to create OTLP exporter", zap.Error(err))
		return nil, nil, err
	}

	tp := sdktrace.NewTracerProvider(
		sdktrace.WithBatcher(exporter),
		sdktrace.WithResource(resource.NewWithAttributes(
			semconv.SchemaURL,
			semconv.ServiceName("transaction-sync-service"),
			semconv.ServiceVersion("1.0.0"),
		)),
	)
	otel.SetTracerProvider(tp)

	shutdown := func() {
		ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
		defer cancel()
		if err := tp.Shutdown(ctx); err != nil {
			logger.Error("failed to shutdown tracker provider", zap.Error(err))
		}
	}

	return tp.Tracer("transaction-sync-service"), shutdown, nil
}

func NewSyncService(kafkaBrokers, topic, consumerGroup string, dbConnStr string, logger *zap.Logger, tracer trace.Tracer, metrics *Metrics) (*SyncService, error) {
	// Create Kafka consumer
	config := &kafka.ConfigMap{
		"bootstrap.servers":       kafkaBrokers,
		"group.id":                consumerGroup,
		"auto.offset.reset":       "earliest",
		"enable.auto.commit":      true,
		"auto.commit.interval.ms": 5000,
		"session.timeout.ms":      6000,
	}

	consumer, err := kafka.NewConsumer(config)
	if err != nil {
		logger.Error("failed to create Kafka consumer", zap.Error(err))
		return nil, err
	}

	// Subscribe to topic
	err = consumer.SubscribeTopics([]string{topic}, nil)
	if err != nil {
		logger.Error("failed to subscribe to topic", zap.Error(err))
		return nil, err
	}

	// Create database connection pool
	dbpool, err := pgxpool.New(context.Background(), dbConnStr)
	if err != nil {
		logger.Error("failed to create database pool", zap.Error(err))
		return nil, err
	}

	// Test the connection
	if err := dbpool.Ping(context.Background()); err != nil {
		logger.Error("failed to ping database", zap.Error(err))
		return nil, err
	}

	return &SyncService{
		consumer:     consumer,
		db:           dbpool,
		logger:       logger,
		tracer:       tracer,
		metrics:      metrics,
		shutdownChan: make(chan os.Signal, 1),
	}, nil
}

func (s *SyncService) Start(ctx context.Context) {
	signal.Notify(s.shutdownChan, syscall.SIGINT, syscall.SIGTERM)

	s.wg.Add(1)
	go s.consumeMessages(ctx)
}

func (s *SyncService) consumeMessages(ctx context.Context) {
	defer s.wg.Done()

	for {
		select {
		case sig := <-s.shutdownChan:
			s.logger.Info("Received shutdown signal", zap.Any("signal", sig))
			return
		default:
			msg, err := s.consumer.ReadMessage(100 * time.Millisecond)
			if err != nil {
				if err.(kafka.Error).Code() != kafka.ErrTimedOut {
					s.logger.Error("Consumer error", zap.Error(err))
				}
				continue
			}

			start := time.Now()
			processCtx, span := s.tracer.Start(ctx, "process_transaction")
			defer span.End()

			if err := s.processMessage(processCtx, msg); err != nil {
				s.logger.Error("Failed to process message", zap.Error(err))
				s.metrics.messagesFailed.Inc()
			} else {
				s.metrics.messagesProcessed.Inc()
			}

			duration := time.Since(start).Seconds()
			s.metrics.processingDuration.Observe(duration)
		}
	}
}

func (s *SyncService) processMessage(ctx context.Context, msg *kafka.Message) error {
	var txn Transaction
	if err := json.Unmarshal(msg.Value, &txn); err != nil {
		s.logger.Error("Failed to unmarshal message", zap.Error(err))
		return err
	}

	s.logger.Info("Processing transaction", zap.String("txn_id", txn.ID))

	start := time.Now()
	defer func() {
		duration := time.Since(start).Seconds()
		s.metrics.dbInsertDuration.Observe(duration)
	}()

	return s.upsertTransaction(ctx, &txn)
}

func (s *SyncService) upsertTransaction(ctx context.Context, txn *Transaction) error {
	span := trace.SpanFromContext(ctx)
	span.SetAttributes(
		semconv.DBOperationKey.String("upsert"),
		semconv.DBNameKey.String("transactions"),
	)

	tx, err := s.db.BeginTx(ctx, pgx.TxOptions{})
	if err != nil {
		s.logger.Error("Failed to begin transaction", zap.Error(err))
		return err
	}
	defer tx.Rollback(ctx)

	// Get or create source
	var sourceID string
	err = tx.QueryRow(ctx,
		`INSERT INTO transactions.sources (name, description, source_type) 
		 VALUES ($1,$2,$3) 
		 ON CONFLICT (name) DO UPDATE SET updated_at = CURRENT_TIMESTAMP 
		 RETURNING id`,
		txn.SourceID,
		txn.Description,
		txn.TransactionType,
	).Scan(&sourceID)
	if err != nil {
		s.logger.Error("Failed to get or create source", zap.Error(err))
		return err
	}

	// Get or create account
	var accountID string
	err = tx.QueryRow(ctx,
		`INSERT INTO transactions.accounts (account_number, account_holder_name, account_type, source_id) 
		 VALUES ($1, $2, $3, $4) 
		 ON CONFLICT (account_number) DO UPDATE SET updated_at = CURRENT_TIMESTAMP 
		 RETURNING id`,
		txn.AccountNumber, txn.AccountHolderName, "checking", sourceID,
	).Scan(&accountID)
	if err != nil {
		s.logger.Error("Failed to upsert account", zap.Error(err))
		return err
	}

	// Get category
	var categoryID *string
	if txn.Category != "" {
		err = tx.QueryRow(ctx,
			"SELECT id FROM transactions.categories WHERE name = $1",
			txn.Category,
		).Scan(&categoryID)
		if err != nil && err != pgx.ErrNoRows {
			s.logger.Error("Failed to get category", zap.Error(err))
			return err
		}
	}

	// Upsert transaction
	_, err = tx.Exec(ctx,
		`INSERT INTO transactions.transactions 
		 (transaction_id, account_id, category_id, description, amount, currency, 
		  transaction_type, merchant, location, reference_number, transaction_timestamp)
		 VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11)
		 ON CONFLICT (account_id, transaction_id, transaction_timestamp) DO UPDATE 
		 SET description = EXCLUDED.description, 
		     category_id = EXCLUDED.category_id,
		     updated_at = CURRENT_TIMESTAMP`,
		txn.ID, accountID, categoryID, txn.Description, txn.Amount, txn.Currency,
		txn.TransactionType, txn.Merchant, txn.Location, txn.ReferenceNumber, txn.TransactionTime,
	)
	if err != nil {
		s.logger.Error("Failed to upsert transaction", zap.Error(err))
		return err
	}

	// Commit the transaction
	if err := tx.Commit(ctx); err != nil {
		s.logger.Error("Failed to commit transaction", zap.Error(err))
		return err
	}

	return nil
}

func (s *SyncService) Shutdown(ctx context.Context) error {
	s.logger.Info("Shutting down sync service")
	s.shutdownChan <- syscall.SIGTERM
	done := make(chan struct{})
	go func() {
		s.wg.Wait()
		close(done)
	}()

	select {
	case <-done:
	case <-ctx.Done():
	}

	if s.consumer != nil {
		s.consumer.Close()
	}
	if s.db != nil {
		s.db.Close()
	}
	s.logger.Info("Sync service shutdown complete")
	return nil
}

func main() {
	// Initialize logger
	logger, _ := zap.NewProduction()
	defer logger.Sync()

	// Load configuration from environment
	kafkaBrokers := os.Getenv("KAFKA_BROKERS")
	if kafkaBrokers == "" {
		kafkaBrokers = "localhost:9092"
	}

	kafkaTopic := os.Getenv("KAFKA_TOPIC")
	if kafkaTopic == "" {
		kafkaTopic = "transactions"
	}

	consumerGroup := os.Getenv("KAFKA_CONSUMER_GROUP")
	if consumerGroup == "" {
		consumerGroup = "sync-service"
	}

	dbConnStr := os.Getenv("DATABASE_URL")
	if dbConnStr == "" {
		dbConnStr = "postgres://admin:admin123!@postgres:5432/transactions"
	}

	jaegerEndpoint := os.Getenv("JAEGER_ENDPOINT")
	if jaegerEndpoint == "" {
		jaegerEndpoint = "http://localhost:4318"
	}

	// Initialize tracing
	tracer, shutdown, err := initTracer(jaegerEndpoint, logger)
	if err != nil {
		logger.Fatal("failed to initialize tracer", zap.Error(err))
	}
	defer shutdown()

	// Initialize metrics
	metrics := newMetrics()
	if err := metrics.Register(); err != nil {
		logger.Fatal("failed to register metrics", zap.Error(err))
	}

	// Start metrics server
	go func() {
		http.Handle("/metrics", promhttp.Handler())
		http.HandleFunc("/health", func(w http.ResponseWriter, r *http.Request) {
			w.WriteHeader(http.StatusOK)
			fmt.Fprintf(w, "OK")
		})
		if err := http.ListenAndServe(":8080", nil); err != nil {
			logger.Error("metrics server error", zap.Error(err))
		}
	}()

	// Create sync service
	syncService, err := NewSyncService(kafkaBrokers, kafkaTopic, consumerGroup, dbConnStr, logger, tracer, metrics)
	if err != nil {
		logger.Fatal("failed to create sync service", zap.Error(err))
	}

	// Start service
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	syncService.Start(ctx)

	// Wait for shutdown
	<-syncService.shutdownChan
	logger.Info("Starting graceful shutdown")

	shutdownCtx, shutdownCancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer shutdownCancel()

	if err := syncService.Shutdown(shutdownCtx); err != nil {
		logger.Error("error during shutdown", zap.Error(err))
	}
}
