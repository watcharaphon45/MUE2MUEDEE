using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Application.Services;

public class PricingService : IPricingService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentStore _currentStore;
    private readonly ICurrentUser _currentUser;

    public PricingService(
        IApplicationDbContext context,
        ICurrentStore currentStore,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentStore = currentStore;
        _currentUser = currentUser;
    }

    private Guid RequireStoreId()
    {
        var storeId = _currentStore.StoreId;
        if (!storeId.HasValue)
            throw AppException.Forbidden("กรุณาเลือกร้านค้าก่อนทำรายการ");
        return storeId.Value;
    }

    public async Task<CostBreakdownDto> GetCostBreakdownAsync(Guid inventoryItemId)
    {
        var storeId = RequireStoreId();
        var item = await _context.InventoryItems
            .Include(i => i.Product)
            .Include(i => i.Costs)
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == inventoryItemId);

        if (item == null)
            throw AppException.NotFound($"ไม่พบรายการสินค้าในคลังรหัส {inventoryItemId}");

        var costItems = item.Costs
            .OrderBy(c => c.IncurredAt)
            .Select(c => new InventoryCostDto(
                c.Id,
                c.InventoryItemId,
                c.CostType,
                FormatCostType(c.CostType),
                c.Amount,
                c.Description,
                c.IncurredAt,
                c.CreatedAt,
                c.CreatedBy
            ))
            .ToList();

        // Calculate breakdown by category
        var purchaseCost = item.Costs.Where(c => c.CostType == CostType.Purchase).Sum(c => c.Amount);
        if (purchaseCost == 0 && item.PurchaseCost > 0)
        {
            // If no explicit Purchase cost item exists yet, use item.PurchaseCost
            purchaseCost = item.PurchaseCost;
        }

        var repairCost = item.Costs.Where(c => c.CostType == CostType.Repair).Sum(c => c.Amount);
        var cleaningCost = item.Costs.Where(c => c.CostType == CostType.Cleaning).Sum(c => c.Amount);
        var shippingCost = item.Costs.Where(c => c.CostType == CostType.Shipping).Sum(c => c.Amount);
        var otherCost = item.Costs.Where(c => c.CostType == CostType.Other).Sum(c => c.Amount);

        var total = item.ActualCost;

        return new CostBreakdownDto(
            item.Id,
            item.Sku,
            item.Product?.ModelName ?? "",
            purchaseCost,
            repairCost,
            cleaningCost,
            shippingCost,
            otherCost,
            total,
            costItems
        );
    }

    public async Task<InventoryCostDto> AddCostItemAsync(Guid inventoryItemId, AddCostItemRequest request)
    {
        var storeId = RequireStoreId();
        var item = await _context.InventoryItems
            .Include(i => i.Costs)
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == inventoryItemId);

        if (item == null)
            throw AppException.NotFound($"ไม่พบรายการสินค้าในคลังรหัส {inventoryItemId}");

        if (request.Amount <= 0)
            throw AppException.BadRequest("จำนวนเงินต้นทุนต้องมากกว่า 0 บาท");

        var cost = new InventoryCost
        {
            StoreId = storeId,
            InventoryItemId = item.Id,
            CostType = request.CostType,
            Amount = request.Amount,
            Description = request.Description?.Trim(),
            IncurredAt = request.IncurredAt ?? DateTimeOffset.UtcNow,
            CreatedBy = _currentUser.Username ?? "system",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.InventoryCosts.Add(cost);

        // Recalculate ActualCost
        var oldCost = item.ActualCost;
        item.ActualCost += request.Amount;

        // Record stock movement for cost adjustment audit
        var movement = new StockMovement
        {
            StoreId = storeId,
            InventoryItemId = item.Id,
            MovementType = StockMovementType.CostAdjustment,
            Quantity = 0,
            Notes = $"บันทึกต้นทุนเพิ่มเติม: +฿{request.Amount:N2} ({FormatCostType(request.CostType)}) [ต้นทุนเดิม: ฿{oldCost:N2} ➔ ใหม่: ฿{item.ActualCost:N2}] {request.Description}".Trim(),
            CreatedBy = _currentUser.Username ?? "system",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.StockMovements.Add(movement);

        await _context.SaveChangesAsync();

        return new InventoryCostDto(
            cost.Id,
            cost.InventoryItemId,
            cost.CostType,
            FormatCostType(cost.CostType),
            cost.Amount,
            cost.Description,
            cost.IncurredAt,
            cost.CreatedAt,
            cost.CreatedBy
        );
    }

    public async Task DeleteCostItemAsync(Guid inventoryItemId, Guid costId)
    {
        var storeId = RequireStoreId();
        var item = await _context.InventoryItems
            .Include(i => i.Costs)
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == inventoryItemId);

        if (item == null)
            throw AppException.NotFound($"ไม่พบรายการสินค้าในคลังรหัส {inventoryItemId}");

        var cost = item.Costs.FirstOrDefault(c => c.Id == costId);
        if (cost == null)
            throw AppException.NotFound($"ไม่พบรายการต้นทุนรหัส {costId}");

        var oldCost = item.ActualCost;
        item.ActualCost = Math.Max(0, item.ActualCost - cost.Amount);

        _context.InventoryCosts.Remove(cost);

        // Record stock movement for deduction audit
        var movement = new StockMovement
        {
            StoreId = storeId,
            InventoryItemId = item.Id,
            MovementType = StockMovementType.CostAdjustment,
            Quantity = 0,
            Notes = $"ยกเลิก/ลบรายการต้นทุน: -฿{cost.Amount:N2} ({FormatCostType(cost.CostType)}) [ต้นทุนเดิม: ฿{oldCost:N2} ➔ ใหม่: ฿{item.ActualCost:N2}] {cost.Description}".Trim(),
            CreatedBy = _currentUser.Username ?? "system",
            CreatedAt = DateTimeOffset.UtcNow
        };
        _context.StockMovements.Add(movement);

        await _context.SaveChangesAsync();
    }

    public async Task<PricingRecommendationDto> GetPricingRecommendationAsync(Guid inventoryItemId, decimal? customTargetMargin = null)
    {
        var storeId = RequireStoreId();
        var item = await _context.InventoryItems
            .Include(i => i.Product).ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == inventoryItemId);

        if (item == null)
            throw AppException.NotFound($"ไม่พบรายการสินค้าในคลังรหัส {inventoryItemId}");

        var store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == storeId);

        // Determine target margin
        decimal targetMargin;
        string marginSource;

        if (customTargetMargin.HasValue && customTargetMargin.Value > 0)
        {
            targetMargin = customTargetMargin.Value;
            marginSource = "กำหนดเอง (Custom)";
        }
        else if (item.Product?.Category?.TargetMargin.HasValue == true && item.Product.Category.TargetMargin.Value > 0)
        {
            targetMargin = item.Product.Category.TargetMargin.Value;
            marginSource = $"หมวดหมู่ {item.Product.Category.Name} ({targetMargin}%)";
        }
        else
        {
            targetMargin = store?.DefaultTargetMargin ?? 15.0m;
            marginSource = $"ค่าเริ่มต้นร้านค้า ({targetMargin}%)";
        }

        var agingDays = Math.Max(0, (int)(DateTimeOffset.UtcNow - item.ReceivedAt).TotalDays);

        var calc = CalculateFormula(item.ActualCost, targetMargin, agingDays);

        return new PricingRecommendationDto(
            item.Id,
            item.Sku,
            item.Product?.ModelName ?? "",
            item.ActualCost,
            item.SellingPrice,
            targetMargin,
            marginSource,
            calc.RecommendedPrice,
            calc.MinimumPrice,
            calc.MaximumPrice,
            calc.BreakEvenPrice,
            calc.EstimatedGrossProfit,
            calc.EstimatedMarginPercentage,
            calc.AgingDays,
            calc.AgingBucket,
            calc.AgingColorCode,
            calc.AgingDiscountPercentage,
            calc.PricingRationale
        );
    }

    public async Task<PriceHistoryDto> UpdatePriceAsync(Guid inventoryItemId, UpdatePriceRequest request)
    {
        var storeId = RequireStoreId();
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.StoreId == storeId && i.Id == inventoryItemId);

        if (item == null)
            throw AppException.NotFound($"ไม่พบรายการสินค้าในคลังรหัส {inventoryItemId}");

        if (request.NewPrice <= 0)
            throw AppException.BadRequest("ราคาขายใหม่ต้องมากกว่า 0 บาท");

        var oldPrice = item.SellingPrice;
        item.SellingPrice = request.NewPrice;

        var history = new PriceHistory
        {
            StoreId = storeId,
            InventoryItemId = item.Id,
            OldPrice = oldPrice,
            NewPrice = request.NewPrice,
            Reason = request.Reason?.Trim() ?? "ปรับราคาขาย",
            ChangedBy = _currentUser.Username ?? "system",
            ChangedAt = DateTimeOffset.UtcNow
        };

        _context.PriceHistories.Add(history);
        await _context.SaveChangesAsync();

        var diff = request.NewPrice - oldPrice;
        return new PriceHistoryDto(
            history.Id,
            history.InventoryItemId,
            history.OldPrice,
            history.NewPrice,
            diff,
            history.Reason,
            history.ChangedBy,
            history.ChangedAt
        );
    }

    public async Task<IReadOnlyList<PriceHistoryDto>> GetPriceHistoriesAsync(Guid inventoryItemId)
    {
        var storeId = RequireStoreId();
        var histories = await _context.PriceHistories
            .Where(ph => ph.StoreId == storeId && ph.InventoryItemId == inventoryItemId)
            .OrderByDescending(ph => ph.ChangedAt)
            .Select(ph => new PriceHistoryDto(
                ph.Id,
                ph.InventoryItemId,
                ph.OldPrice,
                ph.NewPrice,
                ph.NewPrice - ph.OldPrice,
                ph.Reason,
                ph.ChangedBy,
                ph.ChangedAt
            ))
            .ToListAsync();

        return histories;
    }

    public PricingCalculationResult CalculateFormula(decimal actualCost, decimal targetMarginPercent, int agingDays)
    {
        // Guard margins
        var margin = Math.Clamp(targetMarginPercent, 1.0m, 90.0m);
        var marginDecimal = margin / 100.0m;

        // Base Recommended Price = ActualCost / (1 - TargetMargin)
        // e.g. 25,000 / (1 - 0.15) = 25,000 / 0.85 = 29411.7647... -> 29,412 THB
        var baseRecommended = actualCost / (1.0m - marginDecimal);

        // Aging classification & discounts
        string agingBucket;
        string agingColorCode;
        decimal discountPercent;

        if (agingDays <= 30)
        {
            agingBucket = "0–30 วัน (ใหม่)";
            agingColorCode = "emerald";
            discountPercent = 0.0m;
        }
        else if (agingDays <= 60)
        {
            agingBucket = "31–60 วัน (ปกติ)";
            agingColorCode = "emerald";
            discountPercent = 0.0m;
        }
        else if (agingDays <= 90)
        {
            agingBucket = "61–90 วัน (เริ่มนาน)";
            agingColorCode = "amber";
            discountPercent = 5.0m;
        }
        else if (agingDays <= 120)
        {
            agingBucket = "91–120 วัน (ค้างสต็อก)";
            agingColorCode = "orange";
            discountPercent = 10.0m;
        }
        else
        {
            agingBucket = "120+ วัน (สต็อกจม)";
            agingColorCode = "rose";
            discountPercent = 15.0m;
        }

        // Apply aging discount if any
        var discountedRecommended = baseRecommended * (1.0m - (discountPercent / 100.0m));

        // Round to nearest integer (Standard Thai retail rounding)
        var recommendedPrice = Math.Round(discountedRecommended, 0, MidpointRounding.AwayFromZero);

        // Never recommend lower than actual cost (Break-even threshold)
        if (recommendedPrice < actualCost && actualCost > 0)
        {
            recommendedPrice = actualCost;
        }

        // Minimum Price: 5% minimum profit margin
        var minimumPrice = Math.Round(actualCost / (1.0m - 0.05m), 0, MidpointRounding.AwayFromZero);

        // Maximum Price: Target margin + 10% premium
        var maxMarginDecimal = Math.Min(0.80m, marginDecimal + 0.10m);
        var maximumPrice = Math.Round(actualCost / (1.0m - maxMarginDecimal), 0, MidpointRounding.AwayFromZero);

        var estimatedGrossProfit = recommendedPrice - actualCost;
        var estimatedMargin = recommendedPrice > 0
            ? Math.Round((estimatedGrossProfit / recommendedPrice) * 100.0m, 2)
            : 0.0m;

        var rationale = discountPercent > 0
            ? $"คำนวณจากต้นทุนจริง ฿{actualCost:N0} มาร์จิ้นเป้าหมาย {margin}% (ฐาน ฿{baseRecommended:N0}) พร้อมปรับลด {discountPercent}% ตามอายุสต็อก {agingDays} วัน ({agingBucket})"
            : $"คำนวณจากต้นทุนจริง ฿{actualCost:N0} มาร์จิ้นเป้าหมาย {margin}% (ได้กำไร ฿{estimatedGrossProfit:N0} / Margin {estimatedMargin}%)";

        return new PricingCalculationResult(
            actualCost,
            margin,
            recommendedPrice,
            minimumPrice,
            maximumPrice,
            actualCost, // Break-even price
            estimatedGrossProfit,
            estimatedMargin,
            agingDays,
            agingBucket,
            agingColorCode,
            discountPercent,
            rationale
        );
    }

    private static string FormatCostType(CostType type) => type switch
    {
        CostType.Purchase => "ราคารับซื้อสินค้า",
        CostType.Repair => "ค่าซ่อมแซม/เปลี่ยนอะไหล่",
        CostType.Cleaning => "ค่าล้างทำความสะอาด/เซนเซอร์",
        CostType.Shipping => "ค่าจัดส่ง/ขนส่ง",
        CostType.Other => "ค่าใช้จ่ายอื่นๆ",
        _ => type.ToString()
    };
}
