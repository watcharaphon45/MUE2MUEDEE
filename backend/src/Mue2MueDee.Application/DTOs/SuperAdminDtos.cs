namespace Mue2MueDee.Application.DTOs;

public record CreateStoreWithOwnerRequest(
    // Store Information
    string StoreName,
    string? StoreCode,
    string? Phone,
    string? Email,
    string? Address,
    string? PrimaryColor,

    // Owner Information
    string OwnerFullName,
    string OwnerUsername,
    string OwnerEmail,
    string OwnerPassword,
    string? OwnerPhoneNumber
);

public record SuperAdminStoreListItemDto(
    Guid Id,
    string Name,
    string Code,
    string? Phone,
    string? Email,
    string? Address,
    string PrimaryColor,
    bool IsActive,
    DateTimeOffset CreatedAt,
    Guid? OwnerId,
    string? OwnerFullName,
    string? OwnerUsername,
    string? OwnerEmail,
    string? OwnerPhoneNumber,
    int TotalUsersCount
);

public record SuperAdminStatsDto(
    int TotalStores,
    int ActiveStores,
    int TotalOwners,
    int TotalUsers
);
