using Mue2MueDee.Domain.Common;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Domain.Entities;

public class StockMovement : BaseEntity<Guid>, ITenantEntity
{
    public Guid StoreId { get; set; }
    public Guid InventoryItemId { get; set; }
    public StockMovementType MovementType { get; set; }
    public int Quantity { get; set; } // +1 for in, -1 for out
    public string? ReferenceNumber { get; set; } // เช่น เลขที่ใบเสร็จรับเงิน หรือ PO
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public InventoryItem InventoryItem { get; set; } = null!;

    public StockMovement()
    {
        Id = Guid.NewGuid();
    }
}
