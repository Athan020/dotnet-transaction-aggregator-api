
# Create Build Environment
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

COPY ingestion-service/Transaction.Ingestions.Worker/Transaction.Ingestions.Worker.csproj ./ingestion-service/Transaction.Ingestions.Worker/
COPY Shared.Protos/Shared.Protos.csproj ./Shared.Protos/

RUN dotnet restore ingestion-service/Transaction.Ingestions.Worker/Transaction.Ingestions.Worker.csproj

COPY ingestion-service/ ingestion-service/
COPY Shared.Protos/ Shared.Protos/

RUN dotnet publish -c Release -o /app/publish ingestion-service/Transaction.Ingestions.Worker/Transaction.Ingestions.Worker.csproj

# Create Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base


WORKDIR /app

COPY --from=build /app/publish /app

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS=http://*:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT [ "dotnet", "Transaction.Ingestions.Worker.dll" ]

