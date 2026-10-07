namespace Mue2MueDee.Domain.Common;

public interface ITenantEntity
{
    Guid StoreId { get; set; }
}

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
    string? DeletedBy { get; set; }
}

public abstract class BaseEntity<TId>
{
    public TId Id { get; protected set; } = default!;
}

public abstract class AuditableEntity<TId> : BaseEntity<TId>
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
