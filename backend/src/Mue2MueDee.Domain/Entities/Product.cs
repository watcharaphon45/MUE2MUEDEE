using Mue2MueDee.Domain.Common;

namespace Mue2MueDee.Domain.Entities;

public class Product : AuditableEntity<Guid>, ITenantEntity, ISoftDelete
{
    public Guid StoreId { get; set; }
    public Guid BrandId { get; set; }
    public Guid CategoryId { get; set; }

    public string ModelName { get; set; } = string.Empty; // e.g. "Alpha 7 IV (ILCE-7M4)"
    public string ProductCode { get; set; } = string.Empty; // e.g. "SONY-A7M4"
    public string? Description { get; set; }
    public decimal? DefaultSellingPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public Brand Brand { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    public Product()
    {
        Id = Guid.NewGuid();
    }
}
