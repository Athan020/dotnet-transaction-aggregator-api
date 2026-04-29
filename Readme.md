
# Enterprise Transaction Aggregator

The Enterprise Transaction Aggregator API provides a unified interface for aggregating transactions across multiple sources.
It enables customers to retrieve categorized transactions through a queryable HTTP API. This solution makes use of the clean architecture project structure with clear boundaries.


```bash
    .
    ├── Aggregator
    ├── EnterpriseTransactionAggregator.sln
    ├── Readme.md
    ├── Shared.Entities
    ├── Shared.Protos
    ├── categorization-engine
    ├── docker-compose.yml
    ├── docs
    ├── infra-compose.yml
    ├── ingestion-service
    └── scripts
```

## Prerequisites

- **Windows only** Install Visual Studio
    - During installation ensure that the following workloads are selected
        - `ASP.NET and web Development`
- Altenatively make use of your IDE or ode editor of choice.
- Install the current LTS version of .NET [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- Install the latetst version of docker desktop for you system [Docker Desktop](https://www.docker.com/products/docker-desktop/)

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

```bash
    docker compose build
```
## Running the solution

```bash
    docker compose --env-file .env up -d          
```

Teardown

```bash
    docker compose --env-file .env down 
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
