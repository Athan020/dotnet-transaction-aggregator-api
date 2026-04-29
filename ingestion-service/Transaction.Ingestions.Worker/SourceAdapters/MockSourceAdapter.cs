using System;

namespace Transaction.Ingestions.Worker.SourceAdapters;

public sealed class MockSourceAdapter : ISourceAdapter
{
    public string SourceName => "mock";


    private readonly string[] _mockDescriptions =
    [
        "Grocery Shopping",
        "Online Subscription",
        "Utility Bill",
        "Restaurant",
        "Salary",
        "Gift",
        "Car Repair",
        "Medical Expense",
        "FlySaFair Ticket",
        "Amazon Purchase",
        "Netflix Subscription",
        "Gym Membership",
    ];

    public async IAsyncEnumerable<Shared.Entities.Transaction> FetchTransactionsAsync(DateTimeOffset startDate, CancellationToken cancellationToken)
    {
        var numberOfTransactions = Random.Shared.Next(3, 10);

        for (int i = 0; i < numberOfTransactions; i++)
        {
            yield return new Shared.Entities.Transaction
            {
                Amount = (decimal)(Random.Shared.NextDouble() * 1000 - 500), 
                TransactionDate = DateTimeOffset.UtcNow.AddDays(-Random.Shared.Next(0, 90)).DateTime,
                Description = _mockDescriptions[Random.Shared.Next(_mockDescriptions.Length)],
                Currency = "ZAR",
                AccountId = 1,
                CategoryId = null,
                ExternalId = Guid.NewGuid().ToString(),
            };
        }
    }
}
