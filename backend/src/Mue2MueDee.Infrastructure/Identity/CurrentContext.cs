using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Infrastructure.Identity;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}

public class CurrentStore : ICurrentStore
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentStore(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? StoreId
    {
        get
        {
            var storeClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("store_id")?.Value;
            return Guid.TryParse(storeClaim, out var id) ? id : null;
        }
    }

    public StoreRole? Role
    {
        get
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value
                         ?? _httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value;

            if (Enum.TryParse<StoreRole>(roleClaim, true, out var role))
            {
                return role;
            }
            return null;
        }
    }

    public bool HasStoreContext => StoreId.HasValue;
}
