using System;
using System.Collections.Generic;

namespace Shared.Entities;

public partial class Transaction
{
    public Guid Id { get; set; }

    public Guid? SourceId { get; set; }

    public long AccountId { get; set; }

    public string ExternalId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public DateTime TransactionDate { get; set; }

    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Categorization> Categorizations { get; set; } = new List<Categorization>();

    public virtual Category? Category { get; set; }

    public virtual Source? Source { get; set; }
}
