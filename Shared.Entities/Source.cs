using System;
using System.Collections.Generic;

namespace Shared.Entities;

public partial class Source
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? LastSynced { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
