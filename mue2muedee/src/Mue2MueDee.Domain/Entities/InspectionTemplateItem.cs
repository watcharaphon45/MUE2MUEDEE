using Mue2MueDee.Domain.Common;

namespace Mue2MueDee.Domain.Entities;

public class InspectionTemplateItem : BaseEntity<Guid>
{
    public Guid TemplateId { get; set; }
    public string CategoryGroup { get; set; } = string.Empty; // e.g. "เซนเซอร์และภาพถ่าย", "ระบบชัตเตอร์", "สภาพภายนอก"
    public string Title { get; set; } = string.Empty;         // e.g. "ตรวจเช็ค Dead/Hot Pixel", "ตรวจเช็คชัตเตอร์ทุกสปีด"
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsCritical { get; set; }                      // หาก Fail จะส่งผลต่อการรับซื้ออย่างรุนแรง

    // Navigation
    public InspectionTemplate Template { get; set; } = null!;

    public InspectionTemplateItem()
    {
        Id = Guid.NewGuid();
    }
}
