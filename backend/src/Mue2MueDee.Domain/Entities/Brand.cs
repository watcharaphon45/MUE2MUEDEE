using Mue2MueDee.Domain.Common;

namespace Mue2MueDee.Domain.Entities;

public class Brand : AuditableEntity<Guid>, ITenantEntity, ISoftDelete
{
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();

    public Brand()
    {
        Id = Guid.NewGuid();
    }
}
