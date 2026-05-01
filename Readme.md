
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
- Alteratively make use of your IDE or code editor of choice.
- Install the current LTS version of .NET [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- Install the latest version of docker desktop for you system [Docker Desktop](https://www.docker.com/products/docker-desktop/)


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
