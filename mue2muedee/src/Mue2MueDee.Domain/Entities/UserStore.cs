using Mue2MueDee.Domain.Common;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Domain.Entities;

public class UserStore : AuditableEntity<Guid>
{
    public Guid UserId { get; set; }
    public Guid StoreId { get; set; }
    public StoreRole Role { get; set; } = StoreRole.Staff;
    public bool IsActive { get; set; } = true;

    // Navigation
    public User User { get; set; } = null!;
    public Store Store { get; set; } = null!;

    public UserStore()
    {
        Id = Guid.NewGuid();
    }
}
