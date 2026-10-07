namespace Mue2MueDee.Application.DTOs;

public record PublicStoreInfoDto(
    Guid Id,
    string Name,
    string Code,
    string? LogoUrl,
    string PrimaryColor,
    string? Address,
    string? Phone,
    string? Email,
    string? Facebook,
    string? Line,
    string? Website,
    string? OwnerName,
    string? OwnerPhone
);

public record PublicCatalogItemDto(
    Guid Id,
    string Sku,
    string ModelName,
    string BrandName,
    string CategoryName,
    string GradeName,
    string GradeCode,
    decimal SellingPrice,
    int? ShutterCount,
    string? Accessories,
    string? Defects,
    string? Notes,
    string? ImageUrl,
    DateTimeOffset ReceivedAt
);

public record CatalogFilterOptionDto(
    Guid Id,
    string Name,
    string? Code = null
);

public record PublicCatalogFilterMetaDto(
    List<CatalogFilterOptionDto> Brands,
    List<CatalogFilterOptionDto> Categories,
    List<CatalogFilterOptionDto> ConditionGrades,
    int TotalAvailableItems
);

public record CatalogQueryParameters
{
    public string? Search { get; init; }
    public Guid? BrandId { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? ConditionGradeId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? SortBy { get; init; } = "newest"; // "newest", "price_asc", "price_desc", "model"
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 24;
}

public record PublicCatalogResponse(
    PublicStoreInfoDto Store,
    List<PublicCatalogItemDto> Items,
    PublicCatalogFilterMetaDto FilterMeta,
    int TotalCount,
    int CurrentPage,
    int TotalPages
);
