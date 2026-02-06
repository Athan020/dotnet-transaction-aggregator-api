
# Enterprise Transaction Aggregator

The Enterprise Transaction Aggregator API provides a unified interface for aggregating transactions across multiple sources.
It enables customers to retrieve categorized transactions through a queryable HTTP API. This solution makes use of the clean architecture project structure with clear boundaries.


```bash
    .
    ├── Dockerfile
    ├── EnterpriseTransactionAggregator.sln
    ├── Readme.md
    ├── Transaction.Aggregator.Api
    ├── Transaction.Aggregator.Application
    ├── Transaction.Aggregator.Domain
    ├── Transaction.Aggregator.Infrastructure
    └── Transaction.Aggregator.Tests
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
