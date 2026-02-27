FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy the project file
COPY TransactionSource.Simulator.csproj .

# Restore dependencies
RUN dotnet restore "TransactionSource.Simulator.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet build "TransactionSource.Simulator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TransactionSource.Simulator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TransactionSource.Simulator.dll"]
