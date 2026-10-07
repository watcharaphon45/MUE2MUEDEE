using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.DTOs;

public record InventoryCostDto(
    Guid Id,
    Guid InventoryItemId,
    CostType CostType,
    string CostTypeName,
    decimal Amount,
    string? Description,
    DateTimeOffset IncurredAt,
    DateTimeOffset CreatedAt,
    string? CreatedBy
);

public record AddCostItemRequest(
    CostType CostType,
    decimal Amount,
    string? Description,
    DateTimeOffset? IncurredAt
);

public record CostBreakdownDto(
    Guid InventoryItemId,
    string Sku,
    string ModelName,
    decimal PurchaseCost,
    decimal RepairCost,
    decimal CleaningCost,
    decimal ShippingCost,
    decimal OtherCost,
    decimal TotalActualCost,
    IReadOnlyList<InventoryCostDto> CostItems
);

public record PriceHistoryDto(
    Guid Id,
    Guid InventoryItemId,
    decimal OldPrice,
    decimal NewPrice,
    decimal PriceDifference,
    string? Reason,
    string ChangedBy,
    DateTimeOffset ChangedAt
);

public record UpdatePriceRequest(
    decimal NewPrice,
    string? Reason
);

public record PricingRecommendationDto(
    Guid InventoryItemId,
    string Sku,
    string ModelName,
    decimal ActualCost,
    decimal CurrentSellingPrice,
    decimal TargetMarginPercentage,
    string TargetMarginSource,
    decimal RecommendedPrice,
    decimal MinimumPrice,
    decimal MaximumPrice,
    decimal BreakEvenPrice,
    decimal EstimatedGrossProfit,
    decimal EstimatedMarginPercentage,
    int AgingDays,
    string AgingBucket,
    string AgingColorCode,
    decimal AgingDiscountPercentage,
    string PricingRationale
);
