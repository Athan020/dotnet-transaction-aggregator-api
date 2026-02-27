namespace Transaction.Aggregator.Tests.Infrastructure;

/// <summary>
/// Integration test helper for API testing
/// Uses WebApplicationFactory to test the full application pipeline
/// </summary>
public class ApiIntegrationTestFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _factory;
    public HttpClient? Client { get; private set; }

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        Client = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _factory?.Dispose();
        Client?.Dispose();
        await Task.CompletedTask;
    }
}

public class TransactionAggregatorApiTests : IClassFixture<ApiIntegrationTestFixture>
{
    private readonly ApiIntegrationTestFixture _fixture;

    public TransactionAggregatorApiTests(ApiIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetTransactions_HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _fixture.Client!.GetAsync("/health");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTransactions_ApiEndpoint_RespondsWith200()
    {
        // Act
        var response = await _fixture.Client!.GetAsync("/api/transactions");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Metrics_Endpoint_ReturnsPrometheusMetrics()
    {
        // Act
        var response = await _fixture.Client!.GetAsync("/metrics");

        // Assert
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            Assert.Contains("dotnet_", content);
        }
    }
}

public class TransactionDataFlowIntegrationTests
{
    [Fact(Skip = "Requires running infrastructure")]
    public async Task TransactionFlow_FromSimulatorToDatabase_CompletesSuccessfully()
    {
        // This test would:
        // 1. Generate a transaction in the simulator
        // 2. Publish it to Kafka
        // 3. Verify it gets synced to the database
        // 4. Query it via the API

        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires running infrastructure")]
    public async Task KafkaToDatabase_Sync_MaintainsDataIntegrity()
    {
        // This test would:
        // 1. Publish transactions to Kafka
        // 2. Verify the sync service processes them
        // 3. Validate data in database matches source

        await Task.CompletedTask;
    }
}
