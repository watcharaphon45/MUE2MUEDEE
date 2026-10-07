using Mue2MueDee.Domain.Common;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Domain.Entities;

public class SalePayment : BaseEntity<Guid>, ITenantEntity
{
    public Guid StoreId { get; set; }
    public Guid SaleOrderId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }     // e.g. slip reference, transaction id
    public DateTimeOffset PaidAt { get; set; } = DateTimeOffset.UtcNow;
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation
    public Store Store { get; set; } = null!;
    public SaleOrder SaleOrder { get; set; } = null!;

    public SalePayment()
    {
        Id = Guid.NewGuid();
    }
}
