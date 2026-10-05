using Mue2MueDee.Domain.Common;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Domain.Entities;

public class InspectionResult : BaseEntity<Guid>
{
    public Guid PurchaseItemId { get; set; }
    public Guid InspectionTemplateItemId { get; set; }
    public InspectionItemResult Result { get; set; } = InspectionItemResult.Pass;
    public string? Notes { get; set; }
    public string? PhotoUrl { get; set; }

    // Navigation
    public PurchaseItem PurchaseItem { get; set; } = null!;
    public InspectionTemplateItem InspectionTemplateItem { get; set; } = null!;

    public InspectionResult()
    {
        Id = Guid.NewGuid();
    }
}
