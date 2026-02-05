
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
- Aggregation pipeline through service decoration
- Resilient data aggregation
- API rate limiting per IP
- Partial failure management and best effort result sets ensure that a single source failure does not impact the final result set.
- Configurable Json based rule Categorization
- Shared interface for aggregation logic makes it easy to introduce additional sources
- Chaos simulation with Polly to simulate downstream latency (For development purposes)


## Building the solution

- Dotnet cli
    ```ps
        dotnet build
    ```
- Docker cli
    
    ```ps
        doccker build . -t transaction.aggregator:dev
    ```
## Running the solution

- Running the API via the dotnet cli
    - Explicitly run the API project
    ```ps
        dotnet run --project Transaction.Aggregator.Api/Transaction.Aggregator.Api.csproj  
    ``` 
- Running the API via docker desktop cli
    - Run your local image
    ```ps
        docker run --rm -p 5050:5000 transaction.aggregator:dev
    ```

## Testing the solution

- Refer to the [Transactions http file](./Transaction.Aggregator.Api/Transaction.Aggregator.Api.http)

    or

Using `Curl`

- Get Transactions
```ps
    curl -X GET "http://localhost:5027/transactionmanagement/v1/transactions/1"
```

- Pagination

```ps
       curl -X GET "http://localhost:5027/transactionmanagement/v1/transactions/1?PageNumber=1&PageSize=5"
```

To execute unit tests

```ps
    dotnet test
```
## Future enhancements

- Implement propagation of correlation id to downstream sources/services for end-to-end traceability
- Rule Categorization with a persistence layer
- Distributed rate limiting through a persistence layer. Alternatively Rate limiting should be a gateway concern and should instead be managed accordingly.
- Implement caching to minimize the amount calls to downstream services, especially for common requests.