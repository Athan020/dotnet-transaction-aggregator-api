using System;
using System.Collections.Generic;

namespace Shared.Entities;

public partial class Category
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? SubcategoryOf { get; set; }

    public List<string>? Keywords { get; set; }

    public int Version { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Category> InverseSubcategoryOfNavigation { get; set; } = new List<Category>();

    public virtual Category? SubcategoryOfNavigation { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
