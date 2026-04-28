
# Create Build Environment
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

COPY EnterpriseTransactionAggregator.sln .
COPY Transaction.Aggregator.Api/Transaction.Aggregator.Api.csproj ./Transaction.Aggregator.Api/
COPY Transaction.Aggregator.Application/Transaction.Aggregator.Application.csproj ./Transaction.Aggregator.Application/
COPY Transaction.Aggregator.Domain/Transaction.Aggregator.Domain.csproj ./Transaction.Aggregator.Domain/
COPY Transaction.Aggregator.Infrastructure/Transaction.Aggregator.Infrastructure.csproj ./Transaction.Aggregator.Infrastructure/
COPY Transaction.Aggregator.Tests/Transaction.Aggregator.Tests.csproj ./Transaction.Aggregator.Tests/
RUN dotnet restore 

COPY . .

RUN dotnet publish -c Release -o /app/publish Transaction.Aggregator.Api/Transaction.Aggregator.Api.csproj


# Create Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base


WORKDIR /app

COPY --from=build /app/publish /app

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS=http://*:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT [ "dotnet", "Transaction.Aggregator.Api.dll" ]

