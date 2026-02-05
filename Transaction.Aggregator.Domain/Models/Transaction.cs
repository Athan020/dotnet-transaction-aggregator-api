using System;

namespace Transaction.Aggregator.Domain.Models;

public record class TransactionItem
{

    public required string Id { get; set; }

    public decimal? Amount { get; set; }

    public string FromAccountId { get; set; }= null!;

    public DateTime Date { get; set; }

    public string Description { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string Source { get; set; }= null!;
}
