using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.DTOs;

public record LoginRequest(string UsernameOrEmail, string Password, Guid? StoreId = null);

public record SwitchStoreRequest(Guid StoreId);

public record RefreshTokenRequest(string RefreshToken);

public record UserStoreInfoDto(
    Guid StoreId,
    string StoreName,
    string StoreCode,
    string? LogoUrl,
    StoreRole Role,
    string PrimaryColor
);

public record AuthUserDto(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    string? AvatarUrl
);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    AuthUserDto User,
    Guid? CurrentStoreId,
    StoreRole? CurrentRole,
    List<UserStoreInfoDto> AvailableStores
);

public record RegisterRequest(
    string Email,
    string Username,
    string Password,
    string FullName,
    string? PhoneNumber,
    string StoreName,
    string? StoreCode
);
