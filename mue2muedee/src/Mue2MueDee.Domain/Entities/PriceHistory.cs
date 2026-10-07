using Mue2MueDee.Domain.Common;

namespace Mue2MueDee.Domain.Entities;

public class PriceHistory : AuditableEntity<Guid>, ITenantEntity
{
    public Guid StoreId { get; set; }
    public Guid InventoryItemId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public string? Reason { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Store Store { get; set; } = null!;
    public InventoryItem InventoryItem { get; set; } = null!;

    public PriceHistory()
    {
        Id = Guid.NewGuid();
    }
}
