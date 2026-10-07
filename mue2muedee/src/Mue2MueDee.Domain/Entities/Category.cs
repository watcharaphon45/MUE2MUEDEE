using Mue2MueDee.Domain.Common;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Domain.Entities;

public class Category : AuditableEntity<Guid>, ITenantEntity, ISoftDelete
{
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; } = CategoryType.Other;
    public string? Description { get; set; }
    public decimal? TargetMargin { get; set; } // Optional override target margin for this category (e.g. 20%)
    public bool IsActive { get; set; } = true;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();

    public Category()
    {
        Id = Guid.NewGuid();
    }
}
