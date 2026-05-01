using System;
using System.Collections.Generic;

namespace Shared.Entities;

public partial class Categorization
{
    public Guid Id { get; set; }

    public Guid TransactionId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
