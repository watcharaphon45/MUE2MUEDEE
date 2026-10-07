using Mue2MueDee.Domain.Common;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Domain.Entities;

public class SaleOrder : AuditableEntity<Guid>, ITenantEntity, ISoftDelete
{
    public Guid StoreId { get; set; }
    public string SaleNumber { get; set; } = string.Empty; // e.g. "SO-2610-0001"
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTimeOffset SaleDate { get; set; } = DateTimeOffset.UtcNow;
    public SaleStatus Status { get; set; } = SaleStatus.Completed;

    // Financials
    public decimal TotalAmount { get; set; }        // ยอดรวมก่อนหักส่วนลด
    public decimal DiscountAmount { get; set; }      // ส่วนลดรวม
    public decimal NetAmount { get; set; }           // ยอดสุทธิที่เก็บเงินจากลูกค้า
    public decimal TotalCost { get; set; }          // รวมต้นทุนจริงของสินค้าทั้งหมดในบิล
    public decimal TotalGrossProfit { get; set; }   // กำไรขั้นต้นรวม (NetAmount - TotalCost)
    public decimal TotalMarginPercentage { get; set; } // % กำไร (TotalGrossProfit / NetAmount * 100)

    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public Customer? Customer { get; set; }
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<SalePayment> Payments { get; set; } = new List<SalePayment>();

    public SaleOrder()
    {
        Id = Guid.NewGuid();
    }
}
