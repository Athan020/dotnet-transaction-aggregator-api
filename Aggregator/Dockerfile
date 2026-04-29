
# Create Build Environment
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

COPY Aggregator/Transaction.Aggregator.Api/Transaction.Aggregator.Api.csproj ./Aggregator/Transaction.Aggregator.Api/
COPY Aggregator/Transaction.Aggregator.Application/Transaction.Aggregator.Application.csproj ./Aggregator/Transaction.Aggregator.Application/
COPY Aggregator/Transaction.Aggregator.Domain/Transaction.Aggregator.Domain.csproj ./Aggregator/Transaction.Aggregator.Domain/
COPY Aggregator/Transaction.Aggregator.Infrastructure/Transaction.Aggregator.Infrastructure.csproj ./Aggregator/Transaction.Aggregator.Infrastructure/
COPY Aggregator/Transaction.Aggregator.Tests/Transaction.Aggregator.Tests.csproj ./Aggregator/Transaction.Aggregator.Tests/
COPY Shared.Entities/Shared.Entities.csproj ./Shared.Entities/
COPY Shared.Core/Shared.Core.csproj ./Shared.Core/
RUN dotnet restore Aggregator/Transaction.Aggregator.Api/Transaction.Aggregator.Api.csproj

COPY Aggregator/Transaction.Aggregator.Api ./Aggregator/Transaction.Aggregator.Api/
COPY Aggregator/Transaction.Aggregator.Application ./Aggregator/Transaction.Aggregator.Application/
COPY Aggregator/Transaction.Aggregator.Domain ./Aggregator/Transaction.Aggregator.Domain/
COPY Aggregator/Transaction.Aggregator.Infrastructure ./Aggregator/Transaction.Aggregator.Infrastructure/
COPY Aggregator/Transaction.Aggregator.Tests ./Aggregator/Transaction.Aggregator.Tests/
COPY Shared.Entities ./Shared.Entities/
COPY Shared.Core ./Shared.Core/

RUN dotnet publish -c Release -o /app/publish Aggregator/Transaction.Aggregator.Api/Transaction.Aggregator.Api.csproj


# Create Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base


WORKDIR /app

COPY --from=build /app/publish /app

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS=http://*:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT [ "dotnet", "Transaction.Aggregator.Api.dll" ]

