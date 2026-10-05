using Mue2MueDee.Domain.Common;

namespace Mue2MueDee.Domain.Entities;

public class PurchaseItem : AuditableEntity<Guid>, ITenantEntity
{
    public Guid StoreId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public Guid ConditionGradeId { get; set; }

    public int? ShutterCount { get; set; }
    public string? Accessories { get; set; }
    public string? Defects { get; set; }

    // Pricing & Appraisal Breakdown
    public decimal ExpectedSalePrice { get; set; }            // ราคาขายคาดการณ์ (เช่น 30,000)
    public decimal EstimatedRepairCost { get; set; }          // ค่าซ่อมแซมคาดการณ์ (เช่น 1,000)
    public decimal EstimatedCleaningCost { get; set; }        // ค่าทำความสะอาดคาดการณ์ (เช่น 500)
    public decimal MissingAccessoriesDeduction { get; set; }  // ค่าหักกรณีอุปกรณ์ไม่ครบ (เช่น ไม่มีกล่องหัก 500)
    public decimal MaxBuyPrice { get; set; }                  // ราคารับซื้อสูงสุดตามสูตร (เช่น 24,000)
    public decimal OfferedPrice { get; set; }                 // ราคาที่ร้านเสนอซื้อ (เช่น 23,500)
    public decimal AgreedPrice { get; set; }                  // ราคาที่ตกลงกัน (เช่น 23,500)

    // Linked Inventory Item once received
    public Guid? InventoryItemId { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ConditionGrade ConditionGrade { get; set; } = null!;
    public InventoryItem? InventoryItem { get; set; }
    public ICollection<InspectionResult> InspectionResults { get; set; } = new List<InspectionResult>();

    public PurchaseItem()
    {
        Id = Guid.NewGuid();
    }
}
