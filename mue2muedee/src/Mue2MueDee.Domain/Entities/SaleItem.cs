using Mue2MueDee.Domain.Common;

namespace Mue2MueDee.Domain.Entities;

public class SaleItem : BaseEntity<Guid>, ITenantEntity
{
    public Guid StoreId { get; set; }
    public Guid SaleOrderId { get; set; }
    public Guid InventoryItemId { get; set; }
    public Guid ProductId { get; set; }

    // Snapshot Info
    public string ProductModelName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string ConditionGradeCode { get; set; } = string.Empty;

    // Financial Snapshot
    public decimal ActualCost { get; set; }         // Snapshot ต้นทุนจริง ณ วันขาย
    public decimal UnitPrice { get; set; }          // ราคาตั้งขาย
    public decimal Discount { get; set; }           // ส่วนลดเฉพาะชิ้นนี้
    public decimal SubTotal { get; set; }           // ยอดขายสุทธิเฉพาะชิ้นนี้ (UnitPrice - Discount)
    public decimal GrossProfit { get; set; }        // กำไรขั้นต้น (SubTotal - ActualCost)
    public decimal MarginPercentage { get; set; }   // % มาร์จิ้น (GrossProfit / SubTotal * 100)

    public int WarrantyDays { get; set; } = 30;     // รับประกันสินค้า (วัน) เช่น 30 วัน

    // Navigation
    public Store Store { get; set; } = null!;
    public SaleOrder SaleOrder { get; set; } = null!;
    public InventoryItem InventoryItem { get; set; } = null!;
    public Product Product { get; set; } = null!;

    public SaleItem()
    {
        Id = Guid.NewGuid();
    }
}
