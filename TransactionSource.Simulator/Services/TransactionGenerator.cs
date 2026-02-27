using System.Text.Json.Serialization;

namespace TransactionSource.Simulator.Services;

[Serializable]
public record TransactionDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [JsonPropertyName("sourceId")]
    public string SourceId { get; init; } = string.Empty;

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; init; } = string.Empty;

    [JsonPropertyName("accountHolderName")]
    public string AccountHolderName { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = "USD";

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("merchant")]
    public string Merchant { get; init; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; init; } = string.Empty;

    [JsonPropertyName("transactionType")]
    public string TransactionType { get; init; } = "debit";

    [JsonPropertyName("category")]
    public string? Category { get; init; }

    [JsonPropertyName("referenceNumber")]
    public string ReferenceNumber { get; init; } = string.Empty;

    [JsonPropertyName("transactionTime")]
    public DateTime TransactionTime { get; init; }

    [JsonPropertyName("processedAt")]
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
}

public interface ITransactionGenerator
{
    TransactionDto GenerateTransaction(string sourceId);
    IEnumerable<TransactionDto> GenerateTransactionBatch(string sourceId, int batchSize);
}

public class TransactionGenerator : ITransactionGenerator
{
    private static readonly string[] Merchants =
    {
        "Amazon", "Walmart", "Target", "BestBuy", "Apple Store",
        "Starbucks", "McDonald's", "Uber", "Shell Gas", "Whole Foods",
        "Netflix", "Spotify", "Microsoft", "Google", "Facebook",
        "H&M", "Zara", "Nike", "Adidas", "Sephora"
    };

    private static readonly string[] Locations =
    {
        "New York", "Los Angeles", "Chicago", "Houston", "Phoenix",
        "Philadelphia", "San Antonio", "San Diego", "Dallas", "San Jose"
    };

    private static readonly string[] Categories =
    {
        "Groceries", "Utilities", "Entertainment", "Transportation",
        "Healthcare", "Shopping", "Dining", "Other"
    };

    private static readonly string[] Sources = { "card", "prepaid", "reward" };

    private readonly Random _random = new();

    public TransactionDto GenerateTransaction(string sourceId)
    {
        var transactionType = _random.Next(0, 100) < 70 ? "debit" : "credit";
        var amount = (_random.NextDouble() * 1000) + 1; // Amount between 1 and 1001

        return new TransactionDto
        {
            Id = Guid.NewGuid().ToString(),
            SourceId = sourceId,
            AccountNumber = GenerateAccountNumber(),
            AccountHolderName = GenerateAccountHolderName(),
            Amount = (decimal)amount,
            Currency = "USD",
            Description = Merchants[_random.Next(Merchants.Length)],
            Merchant = Merchants[_random.Next(Merchants.Length)],
            Location = Locations[_random.Next(Locations.Length)],
            TransactionType = transactionType,
            Category = Categories[_random.Next(Categories.Length)],
            ReferenceNumber = Guid.NewGuid().ToString()[..12],
            TransactionTime = DateTime.UtcNow.AddMinutes(_random.Next(-60, 0))
        };
    }

    public IEnumerable<TransactionDto> GenerateTransactionBatch(string sourceId, int batchSize)
    {
        var batch = new List<TransactionDto>();
        for (int i = 0; i < batchSize; i++)
        {
            batch.Add(GenerateTransaction(sourceId));
        }
        return batch;
    }

    private string GenerateAccountNumber()
    {
        return string.Join("", Enumerable.Range(0, 16).Select(_ => _random.Next(0, 10)));
    }

    private string GenerateAccountHolderName()
    {
        var firstNames = new[] { "John", "Jane", "Michael", "Sarah", "David", "Emma", "Robert", "Lisa", "William", "Mary" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };

        return $"{firstNames[_random.Next(firstNames.Length)]} {lastNames[_random.Next(lastNames.Length)]}";
    }
}
