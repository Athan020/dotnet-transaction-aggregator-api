
# Create Build Environment
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

COPY categorization-engine/Transaction.Categorization.Engine/Transaction.Categorization.Engine.csproj ./categorization-engine/Transaction.Categorization.Engine/
COPY Shared.Protos/Shared.Protos.csproj ./Shared.Protos/
COPY Shared.Entities/Shared.Entities.csproj ./Shared.Entities/

RUN dotnet restore categorization-engine/Transaction.Categorization.Engine/Transaction.Categorization.Engine.csproj

COPY categorization-engine/ categorization-engine/
COPY Shared.Protos/ Shared.Protos/
COPY Shared.Entities/ Shared.Entities/

RUN dotnet publish -c Release -o /app/publish categorization-engine/Transaction.Categorization.Engine/Transaction.Categorization.Engine.csproj

# Create Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base


WORKDIR /app

COPY --from=build /app/publish /app

EXPOSE 5000/tcp
ENV ASPNETCORE_URLS=http://*:5000

ENTRYPOINT [ "dotnet", "Transaction.Categorization.Engine.dll" ]

