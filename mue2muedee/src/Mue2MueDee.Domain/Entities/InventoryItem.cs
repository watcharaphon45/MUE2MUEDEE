using Mue2MueDee.Domain.Common;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Domain.Entities;

public class InventoryItem : AuditableEntity<Guid>, ITenantEntity, ISoftDelete
{
    public Guid StoreId { get; set; }
    public string Sku { get; set; } = string.Empty; // Unique SKU e.g. "CAM-2610-0001"
    public Guid ProductId { get; set; }
    public string SerialNumber { get; set; } = string.Empty; // หมายเลขซีเรียลประจำตัวกล้อง/เลนส์
    public Guid ConditionGradeId { get; set; }
    public InventoryStatus Status { get; set; } = InventoryStatus.Available;

    // Financials
    public decimal PurchaseCost { get; set; } // ราคารับซื้อเข้า
    public decimal ActualCost { get; set; }   // ต้นทุนจริง (ค่ารับซื้อ + ค่าซ่อม/ทำความสะอาด/ขนส่ง)
    public decimal SellingPrice { get; set; } // ราคาตั้งขาย

    // Camera/Lens Specifics
    public int? ShutterCount { get; set; }     // จำนวนชัตเตอร์ (สำหรับกล้อง)
    public string? Accessories { get; set; }   // อุปกรณ์ที่มาด้วย (เช่น ครบกล่อง, แบตแท้ 2 ก้อน, แท่นชาร์จ)
    public string? Defects { get; set; }       // ตำหนิหรือริ้วรอย
    public string? Notes { get; set; }         // หมายเหตุภายในร้าน

    // Dates & Origins
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SoldAt { get; set; }
    public Guid? CustomerId { get; set; }      // ผู้ขายเครื่องนี้ให้ร้าน

    // Concurrency Token
    public byte[]? RowVersion { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ConditionGrade ConditionGrade { get; set; } = null!;
    public Customer? Customer { get; set; }
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<InventoryCost> Costs { get; set; } = new List<InventoryCost>();
    public ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
    public ICollection<InventoryItemImage> Images { get; set; } = new List<InventoryItemImage>();

    public InventoryItem()
    {
        Id = Guid.NewGuid();
    }
}
