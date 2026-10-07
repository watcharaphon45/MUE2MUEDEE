using Mue2MueDee.Domain.Common;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Domain.Entities;

public class PurchaseOrder : AuditableEntity<Guid>, ITenantEntity, ISoftDelete
{
    public Guid StoreId { get; set; }
    public string PoNumber { get; set; } = string.Empty; // e.g. "PO-2610-0001"
    public Guid CustomerId { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Draft;

    // Financial Totals
    public decimal TotalEstimatedValue { get; set; }  // มูลค่าประเมินขายรวม
    public decimal TotalMaxBuyPrice { get; set; }      // ราคารับซื้อสูงสุดที่ระบบแนะนำรวม
    public decimal TotalOfferedPrice { get; set; }     // ราคาที่ร้านเสนอซื้อรวม
    public decimal TotalAgreedPrice { get; set; }      // ราคาที่ตกลงรับซื้อจริงรวม

    // Approval (if OfferedPrice > MaxBuyPrice)
    public bool NeedsApproval { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }

    public string? Notes { get; set; }
    public DateTimeOffset PurchaseDate { get; set; } = DateTimeOffset.UtcNow;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
    public ICollection<PurchasePayment> Payments { get; set; } = new List<PurchasePayment>();

    public PurchaseOrder()
    {
        Id = Guid.NewGuid();
    }
}
