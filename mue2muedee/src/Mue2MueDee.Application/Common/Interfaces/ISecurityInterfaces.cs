using System.Security.Claims;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, Guid? storeId, StoreRole? role);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
