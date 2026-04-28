using System;

namespace Transaction.Aggregator.Domain.Models;

public record class TransactionItem
{

    public required string Id { get; set; }

    public decimal? Amount { get; set; }

    public long? FromAccountId { get; set; }

    public DateTime Date { get; set; }

    public string Description { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string Source { get; set; }= null!;
}
