using Mue2MueDee.Domain.Common;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Domain.Entities;

public class InspectionTemplate : AuditableEntity<Guid>, ITenantEntity, ISoftDelete
{
    public Guid StoreId { get; set; }
    public CategoryType CategoryType { get; set; } // Camera, Lens, etc.
    public string Name { get; set; } = string.Empty; // e.g. "แบบฟอร์มตรวจเช็คสภาพตัวกล้องถ่ายภาพ"
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public ICollection<InspectionTemplateItem> Items { get; set; } = new List<InspectionTemplateItem>();

    public InspectionTemplate()
    {
        Id = Guid.NewGuid();
    }
}
