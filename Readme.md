
# Enterprise Transaction Aggregator

The Enterprise Transaction Aggregator API provides a unified interface for aggregating transactions across multiple sources.
It enables users to retrieve categorized transactions through a queryable HTTP API.


## Prerequisites

- **Windows only** Install Visual Studio
    - During installation ensure that the following workloads are selected
        - `ASP.NET and web Development`
- Install the current LTS version of .NET [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

- Install the latetst version of docker desktop for you system [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Building the solution


## Running the solution

- Running the API via the dotnet cli
    - Explicitly run the API project
    ```ps
        dotnet run --project Transaction.Aggregator.Api/Transaction.Aggregator.Api.csproj  
    ``` 
- Running the API via docker desktop cli
    - Build your image
    
    ```ps
        doccker build . -t transaction.aggregator:dev
    ```
    - Run your local image
    ```ps
        docker run --rm -p 5050:5000 transaction.aggregator:dev
    ```

## Testing the solution
