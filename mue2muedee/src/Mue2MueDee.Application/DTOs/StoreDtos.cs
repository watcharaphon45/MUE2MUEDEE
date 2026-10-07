using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.DTOs;

public record StoreDto(
    Guid Id,
    string Name,
    string Code,
    string? LogoUrl,
    string? Phone,
    string? Email,
    string? Address,
    string? TaxId,
    string? Facebook,
    string? Line,
    string? Website,
    string PrimaryColor,
    bool IsActive,
    DateTimeOffset CreatedAt
);

public record UpdateStoreRequest(
    string Name,
    string? LogoUrl,
    string? Phone,
    string? Email,
    string? Address,
    string? TaxId,
    string? Facebook,
    string? Line,
    string? Website,
    string PrimaryColor
);

public record StoreUserDto(
    Guid UserId,
    string Username,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? AvatarUrl,
    StoreRole Role,
    bool IsActive,
    DateTimeOffset JoinedAt
);

public record AddUserToStoreRequest(
    string Email,
    string FullName,
    string? PhoneNumber,
    string TemporaryPassword,
    StoreRole Role
);

public record UpdateUserStoreRoleRequest(
    StoreRole Role,
    bool IsActive
);
