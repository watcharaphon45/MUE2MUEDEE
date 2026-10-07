using Mue2MueDee.Domain.Common;

namespace Mue2MueDee.Domain.Entities;

public class ConditionGrade : AuditableEntity<Guid>, ITenantEntity, ISoftDelete
{
    public Guid StoreId { get; set; }
    public string GradeCode { get; set; } = string.Empty; // e.g. "A+", "A", "B+", "B", "C"
    public string GradeName { get; set; } = string.Empty; // e.g. "สภาพนางฟ้า (เหมือนใหม่ ไร้ริ้วรอย)", "สภาพสวยงาม", "มีร่องรอยการใช้งาน"
    public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    public ConditionGrade()
    {
        Id = Guid.NewGuid();
    }
}
