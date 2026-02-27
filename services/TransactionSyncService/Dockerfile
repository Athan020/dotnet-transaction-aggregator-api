# Build stage
FROM golang:1.22-bookworm AS builder

WORKDIR /app

# Install build dependencies (librdkafka-dev for Kafka bindings)
RUN apt-get update && apt-get install -y --no-install-recommends \
    git \
    gcc \
    librdkafka-dev \
    pkg-config \
    && rm -rf /var/lib/apt/lists/*

# Copy go mod files
COPY go.mod ./
COPY go.sum* ./

# Download dependencies and generate go.sum
RUN go mod download && go mod tidy

# Copy source code
COPY . .

# Build the application
RUN CGO_ENABLED=1 GOOS=linux go build -a -installsuffix cgo -o transaction-sync-service .

# Final stage
FROM debian:bookworm-slim

WORKDIR /root/

# Install runtime dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
    librdkafka1 \
    && rm -rf /var/lib/apt/lists/*

# Copy the binary from builder
COPY --from=builder /app/transaction-sync-service .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:8080/health || exit 1

# Environment variables
ENV DATABASE_URL=""
ENV KAFKA_BROKERS="kafka:29092"
ENV KAFKA_TOPIC="transactions"
ENV KAFKA_CONSUMER_GROUP="sync-service"
ENV JAEGER_ENDPOINT="http://jaeger:4318"
ENV LOG_LEVEL="info"

EXPOSE 8080

CMD ["./transaction-sync-service"]
