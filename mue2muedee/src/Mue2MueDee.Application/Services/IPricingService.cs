using Mue2MueDee.Application.DTOs;

namespace Mue2MueDee.Application.Services;

public record PricingCalculationResult(
    decimal ActualCost,
    decimal TargetMarginPercent,
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

public interface IPricingService
{
    Task<CostBreakdownDto> GetCostBreakdownAsync(Guid inventoryItemId);
    Task<InventoryCostDto> AddCostItemAsync(Guid inventoryItemId, AddCostItemRequest request);
    Task DeleteCostItemAsync(Guid inventoryItemId, Guid costId);
    Task<PricingRecommendationDto> GetPricingRecommendationAsync(Guid inventoryItemId, decimal? customTargetMargin = null);
    Task<PriceHistoryDto> UpdatePriceAsync(Guid inventoryItemId, UpdatePriceRequest request);
    Task<IReadOnlyList<PriceHistoryDto>> GetPriceHistoriesAsync(Guid inventoryItemId);
    PricingCalculationResult CalculateFormula(decimal actualCost, decimal targetMarginPercent, int agingDays);
}
