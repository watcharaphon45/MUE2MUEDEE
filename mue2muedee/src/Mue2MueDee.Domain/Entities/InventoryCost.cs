using Mue2MueDee.Domain.Common;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Domain.Entities;

public class InventoryCost : AuditableEntity<Guid>, ITenantEntity
{
    public Guid StoreId { get; set; }
    public Guid InventoryItemId { get; set; }
    public CostType CostType { get; set; } = CostType.Other;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset IncurredAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Store Store { get; set; } = null!;
    public InventoryItem InventoryItem { get; set; } = null!;

    public InventoryCost()
    {
        Id = Guid.NewGuid();
    }
}
