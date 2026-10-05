using Mue2MueDee.Domain.Common;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Domain.Entities;

public class PurchasePayment : BaseEntity<Guid>, ITenantEntity
{
    public Guid StoreId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;
    public decimal Amount { get; set; }
    public DateTimeOffset PaidAt { get; set; } = DateTimeOffset.UtcNow;
    public string? ReferenceNumber { get; set; } // เช่น เลขที่สลิปโอนเงิน
    public string? ProofUrl { get; set; }        // รูปสลิป
    public string? Notes { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    public PurchasePayment()
    {
        Id = Guid.NewGuid();
    }
}
