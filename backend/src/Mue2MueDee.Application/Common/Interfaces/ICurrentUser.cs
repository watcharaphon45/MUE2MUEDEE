using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}

public interface ICurrentStore
{
    Guid? StoreId { get; }
    StoreRole? Role { get; }
    bool HasStoreContext { get; }
}
