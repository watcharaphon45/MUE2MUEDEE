using Mue2MueDee.Domain.Common;

namespace Mue2MueDee.Domain.Entities;

public class Store : AuditableEntity<Guid>, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxId { get; set; }
    public string? Facebook { get; set; }
    public string? Line { get; set; }
    public string? Website { get; set; }
    public string PrimaryColor { get; set; } = "#4f46e5";
    public decimal DefaultTargetMargin { get; set; } = 15.0m; // Default target profit margin (e.g. 15%)
    public bool IsActive { get; set; } = true;

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public ICollection<UserStore> UserStores { get; set; } = new List<UserStore>();

    public Store()
    {
        Id = Guid.NewGuid();
    }
}
