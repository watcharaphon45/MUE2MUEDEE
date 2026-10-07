using Mue2MueDee.Domain.Common;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Domain.Entities;

public class Customer : AuditableEntity<Guid>, ITenantEntity, ISoftDelete
{
    public Guid StoreId { get; set; }
    public CustomerType CustomerType { get; set; } = CustomerType.Both;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? IdCardNumber { get; set; } // เลขบัตรประชาชน (สำคัญมากสำหรับสัญญาซื้อขายของมือสอง)
    public string? Address { get; set; }
    public string? LineId { get; set; }
    public string? Facebook { get; set; }

    // Bank Account for purchasing gear from customer
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }

    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public ICollection<InventoryItem> SourcedInventoryItems { get; set; } = new List<InventoryItem>();

    public Customer()
    {
        Id = Guid.NewGuid();
    }
}
