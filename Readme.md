
# Enterprise Transaction Aggregator API

A production-grade, event-driven transaction aggregation platform with distributed tracing, observability, and horizontal scalability.

## 📋 Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Quick Start](#quick-start)
- [Components](#components)
- [Deployment](#deployment)
- [Monitoring](#monitoring)
- [Development](#development)
- [Full Documentation](#full-documentation)

## ✨ Key Features

- **Event-Driven Architecture**: Kafka-based event streaming for real-time transaction processing
- **Distributed Tracing**: Jaeger integration for end-to-end request tracing
- **Metrics & Observability**: Prometheus metrics with Grafana dashboards
- **Horizontal Scalability**: Stateless services that scale independently
- **Data Consistency**: PostgreSQL with normalized schema and ACID transactions
- **High-Performance Caching**: Redis L1/L2 cache layer
- **Automatic Data Sync**: Go-based sync service for Kafka→Database ingestion
- **Transaction Simulation**: Generate realistic transaction data from multiple sources
- **Comprehensive Testing**: Unit and integration tests with high coverage
- **Containerized**: Production-ready Docker & Docker Compose setup

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────────────────┐
│         Transaction Aggregator Ecosystem                     │
└──────────────────────────────────────────────────────────────┘

┌─────────────────────┐     ┌──────────────────┐    ┌────────────┐
│  API Service        │     │  Simulator       │    │  Sync      │
│  (.NET 10)          │────▶│  Workers         │──▶ │  Service   │
│  • REST API         │     │  (.NET 10)       │    │  (Go)      │
│  • Aggregation      │     │  • Generates     │    │  • Consumes│
│  • Caching          │     │  • Multi-source  │    │  • Syncs   │
└─────────────────────┘     └──────────────────┘    └────────────┘
         │                          │                      │
         │                          │                      │
         └──────────────┬───────────┴──────────────┬───────┘
                        │ Events                   │
                        │ (JSON/Avro)              │
                  ┌─────▼────────────────────┐    │
                  │  Kafka                   │    │
                  │  • High-throughput       │    │
                  │  • Fault-tolerant        │    │
                  │  • Consumer groups       │    │
                  └──────────────────────────┘    │
                                                  │
        ┌─────────────────────┬──────────┬────────┴──────────┐
        │                     │          │                   │
   ┌────▼──────┐        ┌────▼──┐  ┌───▼────┐         ┌────▼────┐
   │ PostgreSQL │        │ Redis │  │ Jaeger │         │Prometheus
   │ Database   │        │ Cache │  │ Tracing          │ Metrics
   │ • Normalized        │       │  │        │         │ • Sched
   │ • Indexed  │        │       │  │        │         │ • Storage
   └───────────┘        │       │  │        │         │
                        │       │  │        │         │
                  ┌─────▼──────▼───▼────────▼─────────▼────┐
                  │         Grafana Dashboards             │
                  │         • Real-time metrics             │
                  │         • Alerts & Notifications        │
                  └────────────────────────────────────────┘
```

## 🚀 Quick Start

### Prerequisites

- [Docker & Docker Compose v2.0+](https://www.docker.com/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Go 1.22+ (optional, for sync service development)](https://golang.org/)

### Get Started in 5 Minutes

```bash
# 1. Clone and navigate
git clone <repository>
cd EnterpriseTransactionAggregator

# 2. Start infrastructure (required for all services)
.\setup.sh infra-up
# or on Windows: 
docker-compose -f docker-compose.infrastructure.yml up -d

# 3. Start application services
.\setup.sh up
# or on Windows:
docker-compose up -d

# 4. Verify status
.\setup.sh status

# 5. Access the services
echo "API: http://localhost:5050"
echo "Grafana: http://localhost:3000 (admin/admin123)"
echo "Jaeger: http://localhost:16686"
```

### Available URLs

| Service | URL |
|---------|-----|
| **API** | http://localhost:5050 |
| **OpenAPI Docs** | http://localhost:5050/scalar/v1 |
| **Health Check** | http://localhost:5050/health |
| **Prometheus** | http://localhost:9090 |
| **Grafana** | http://localhost:3000 |
| **Jaeger UI** | http://localhost:16686 |
| **Kafka UI** | http://localhost:8080 |
| **PgAdmin** | http://localhost:8888 |

## 📦 Project Structure

```
.
├── docker-compose.yml                    # Application services
├── docker-compose.infrastructure.yml     # Infrastructure (Kafka, DB, etc)
├── setup.sh                              # Helper script
├── PRODUCTION_SETUP.md                   # Detailed deployment guide
├── Transaction.Aggregator.Api/           # REST API Service
│   ├── Controllers/
│   ├── Extensions/                       # OpenTelemetry, Health Checks
│   ├── Middleware/                       # Exception Handling, Tracing
│   └── Program.cs
├── Transaction.Aggregator.Application/   # Business Logic
│   ├── TransactionAggregator.cs
│   ├── TransactionManager.cs
│   └── CategorizationAggregator.cs
├── Transaction.Aggregator.Infrastructure/ # Data Access
│   ├── Sources/                          # Transaction Sources
│   ├── RuleEngine/                       # Categorization Rules
│   └── Services/
├── Transaction.Aggregator.Domain/        # Domain Models
│   └── Models/
├── Transaction.Aggregator.Tests/         # Unit & Integration Tests
├── TransactionSource.Simulator/          # Kafka Producer
│   └── Services/
├── services/TransactionSyncService/      # Go Consumer Service
│   └── main.go
├── config/                               # Configuration files
│   ├── prometheus.yml
│   └── grafana/
└── scripts/
    └── init-db.sql                       # Database schema
```

## 🔧 Components

### Transaction.Aggregator.Api
**Purpose**: REST API for querying transactions  
**Technology**: .NET 10, ASP.NET Core  
**Port**: 5050

**Endpoints**:
- `GET /api/transactions` - Query transactions with filters
- `GET /health` - Health check
- `GET /metrics` - Prometheus metrics

**Features**:
- Multi-source aggregation
- Hybrid caching (Redis + in-memory)
- OpenTelemetry tracing
- Prometheus metrics export
- Rate limiting per IP
- Exception handling

### TransactionSource.Simulator  
**Purpose**: Generate and publish transaction events  
**Technology**: .NET 10 Worker Service  
**Publishes To**: Kafka

**Features**:
- Realistic transaction generation
- Configurable batch size & frequency
- Multiple source simulation
- OpenTelemetry tracing
- Production-ready error handling

**Configuration**:
```json
{
  "SimulatorConfig": {
    "TransactionBatchSize": 10,      // Transactions per batch
    "BatchIntervalSeconds": 30,       // Publish frequency (in seconds)
    "NumberOfSources": 3              // Number of sources to simulate
  }
}
```

### TransactionSyncService
**Purpose**: Consume Kafka events and sync to PostgreSQL  
**Technology**: Go 1.22  
**Port**: 8080  
**Consumes From**: Kafka `transactions` topic

**Features**:
- High-performance event processing
- Batch operations for efficiency
- Distributed tracing
- Prometheus metrics
- Graceful shutdown
- Offset management

### PostgreSQL Database
**Purpose**: Persistent transaction storage  
**Port**: 5432  
**Default Credentials**: admin/admin123!  
**Database**: transactions

**Schema**:
- `transactions.sources` - Transaction sources
- `transactions.accounts` - Customer accounts
- `transactions.transactions` - Transaction records
- `transactions.categories` - Transaction categories
- `transactions.sync_status` - Kafka sync tracker
- `audit.transaction_changes` - Audit log

### Redis Cache
**Purpose**: High-speed caching layer  
**Port**: 6379  
**Configuration**: 512MB with LRU eviction

### Kafka
**Purpose**: Event streaming backbone  
**Port**: 9092 (broker), 2181 (zookeeper)  
**Topic**: `transactions`  
**Partitions**: 3  
**Replication**: 1

### Observability Stack

#### Jaeger
**Port**: 16686 (UI), 14268 (endpoint)  
**Purpose**: Distributed tracing  
**Features**:
- Service dependency mapping
- Latency analysis
- Error tracking
- Performance visualization

#### Prometheus
**Port**: 9090  
**Purpose**: Time-series metrics collection  
**Features**:
- 15-second scrape interval
- 15-day retention
- Query language (PromQL)

#### Grafana
**Port**: 3000  
**Default**: admin/admin123  
**Purpose**: Metrics visualization  
**Features**:
- Pre-configured dashboards
- Real-time monitoring
- Alert management

## 📊 Monitoring & Observability

### Key Metrics

```
# Transaction API
transaction_api_requests_total{method="GET", endpoint="/api/transactions"}
transaction_api_latency_ms{endpoint="/api/transactions", percentile="p99"}
transaction_api_cache_hits_total
transaction_api_cache_misses_total

# Simulator
transaction_simulator_messages_published_total
transaction_simulator_publish_latency_ms

# Sync Service
transaction_sync_messages_processed_total
transaction_sync_processing_duration_seconds
transaction_sync_lag
transaction_sync_failures_total

# Infrastructure
kafka_consumer_lag
postgres_connections
redis_memory_usage
```

### Viewing Traces

1. Open http://localhost:16686
2. Select service from dropdown
3. View traces with full spans
4. Analyze latencies and dependencies

## 🧪 Testing

### Run All Tests
```bash
dotnet test Transaction.Aggregator.Tests

# OR using setup script
./setup.sh test
```

### Run with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# OR using setup script
./setup.sh test-coverage
```

### Test Categories
- **Unit Tests**: Individual component testing
- **Integration Tests**: API endpoint testing
- **Data Flow Tests**: End-to-end Kafka→DB tests

## 🔨 Development

### Local Setup
```bash
# Install dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run API locally (requires infrastructure)
cd Transaction.Aggregator.Api
dotnet run
```

### Adding a New Transaction Source

1. Create class in `Transaction.Aggregator.Infrastructure/Sources/`
2. Implement `ITransactionSource`
3. Register in `Program.cs`
4. Add tests

### Database Migrations

The schema is initialized via `scripts/init-db.sql`. For local changes:

```bash
# Connect to database
docker-compose -f docker-compose.infrastructure.yml exec postgres psql -U admin -d transactions

# Run migration
\i scripts/migration-name.sql
```

## 🚢 Deployment

### Docker Compose (Development/Testing)

```bash
# Infrastructure only
docker-compose -f docker-compose.infrastructure.yml up -d

# All services
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f
```

### Environment Variables

```bash
# Docker Compose uses environment variables from:
# - .env file (if present)
# - docker-compose.yml environment sections
# - Command line: docker-compose -e VAR=value up
```

### Production Considerations

See [PRODUCTION_SETUP.md](./PRODUCTION_SETUP.md) for:
- Security hardening
- Performance tuning
- Scaling strategies
- Backup & restore
- Monitoring setup

## 📚 Full Documentation

For comprehensive documentation, see:
- [PRODUCTION_SETUP.md](./PRODUCTION_SETUP.md) - Complete deployment & operations guide
- [API Documentation](http://localhost:5050/scalar/v1) - Interactive API docs
- [Jaeger Documentation](https://www.jaegertracing.io/docs/) - Distributed tracing
- [Prometheus Query Language](https://prometheus.io/docs/prometheus/latest/querying/basics/) - Metrics queries

## 🛠️ Troubleshooting

## Key features

- Fan-out like data aggregation for transactions across multiple sources
- Per transaction source caching
- Fail safe mechanism 
- Global Exception handling
- Aggregation pipeline through service decoration
- Resilient data aggregation through service decoration
- API rate limiting per IP
- Partial failure management and best effort result sets ensure that a single source failure does not impact the final result set.
- Configurable Json based rule categorization
- Shared interface for aggregation logic makes it easy to introduce additional sources
- Chaos simulation with Polly to simulate downstream latency (For development purposes)

```
    ┌─────────────────────────────┐
    │ Transactions Controller     │
    └────────────┬────────────────┘
                 │
    ┌────────────▼─────────────────┐
    │ Categorization Engine        │
    └────────────┬─────────────────┘
                 │
    ┌────────────▼──────────────────────┐
    │ Transaction Aggregator            │
    └────────────┬──────────────────────┘
                 │
    ┌────────────▼─────────────────┐
    │ Hybrid Cache (L1/L2)         │
    └────────────┬─────────────────┘
                 │
    ┌────────────▼──────────────────────┐
    │ Resilience Pipeline               │
    │ (Retry, Timeout, Circuit Breaker) │
    └────────────┬──────────────────────┘
                 │
         _________┴________
        │                 │
    ┌───▼────────┐  ┌─────▼────────┐
    │ Source 1   │  │ Source 2     │
    │ (Card)     │  │ (Prepaid)    │
    └────────────┘  └──────────────┘
```



## Building the solution

```ps
    docker-compose build
```
## Running the solution

```ps
    docker-compose up -d           
```

Teardown

```ps
    docker-compose down
```

## Testing the solution

- Refer to the [Transactions http file](./Transaction.Aggregator.Api/Transaction.Aggregator.Api.http)

    or

Using `Curl`

- Get Transactions
```ps
    curl -X GET "http://localhost:5050/transactionmanagement/v1/transactions/1" -s | jq
```

- Pagination

```ps
    curl -X GET "http://localhost:5050/transactionmanagement/v1/transactions/1?PageNumber=1&PageSize=5" -s | jq
```

To execute unit tests

```ps
    dotnet test
```
## Future enhancements

- Implement propagation of correlation id to downstream sources/services for end-to-end traceability
- Rule Categorization with a persistence layer
- Distributed rate limiting through a persistence layer. Alternatively Rate limiting should be a gateway concern and should instead be managed accordingly.
- Persistence of circuit breaker state to allow for distributed circuit breakers
- Implement back plane to ensure distributed fallback cache mechanism behaves the same across all instances 
