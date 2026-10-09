using Mue2MueDee.Domain.Common;

namespace Mue2MueDee.Domain.Entities;

/// <summary>
/// รูปภาพประจำชิ้นสินค้า (1 ชิ้นมีได้ไม่จำกัดรูป) — SortOrder = 0 คือรูปปก
/// </summary>
public class InventoryItemImage : AuditableEntity<Guid>, ITenantEntity
{
    public Guid StoreId { get; set; }
    public Guid InventoryItemId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public InventoryItem InventoryItem { get; set; } = null!;

    public InventoryItemImage()
    {
        Id = Guid.NewGuid();
    }
}
