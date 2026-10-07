using Mue2MueDee.Domain.Common;

namespace Mue2MueDee.Domain.Entities;

public class AuditLog : BaseEntity<Guid>
{
    public Guid? StoreId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, CHANGE_PRICE, etc.
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? IpAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public AuditLog()
    {
        Id = Guid.NewGuid();
    }
}
