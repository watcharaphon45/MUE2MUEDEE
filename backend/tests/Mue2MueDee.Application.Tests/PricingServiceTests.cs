using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;
using Mue2MueDee.Infrastructure.Persistence;
using Xunit;

namespace Mue2MueDee.Application.Tests;

public class PricingServiceTests
{
    private (ApplicationDbContext Context, PricingService Service, Guid StoreId, InventoryItem Item) SetupTestEnvironment()
    {
        var store = new TestCurrentStore();
        var user = new TestCurrentUser();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options, store, user);

        var storeEntity = new Store
        {
            Name = "ABC Camera",
            Code = "ABC",
            DefaultTargetMargin = 15.0m
        };
        store.StoreId = storeEntity.Id;
        context.Stores.Add(storeEntity);

        var brand = new Brand { StoreId = store.StoreId!.Value, Name = "Sony" };
        var category = new Category { StoreId = store.StoreId!.Value, Name = "Camera", Type = CategoryType.Camera, TargetMargin = 15.0m };
        var grade = new ConditionGrade { StoreId = store.StoreId!.Value, GradeCode = "A", GradeName = "ดีมาก", SortOrder = 1 };
        var product = new Product
        {
            StoreId = store.StoreId!.Value,
            BrandId = brand.Id,
            CategoryId = category.Id,
            ModelName = "A7 III",
            ProductCode = "SONY-A7M3",
            DefaultSellingPrice = 32000m
        };

        context.Brands.Add(brand);
        context.Categories.Add(category);
        context.ConditionGrades.Add(grade);
        context.Products.Add(product);

        var item = new InventoryItem
        {
            StoreId = store.StoreId!.Value,
            Sku = "CAM-2610-0001",
            ProductId = product.Id,
            SerialNumber = "SN-SONY-7788",
            ConditionGradeId = grade.Id,
            Status = InventoryStatus.Available,
            PurchaseCost = 24000m,
            ActualCost = 25000m,
            SellingPrice = 30000m,
            ReceivedAt = DateTimeOffset.UtcNow.AddDays(-10)
        };
        context.InventoryItems.Add(item);
        context.SaveChanges();

        var service = new PricingService(context, store, user);
        return (context, service, store.StoreId!.Value, item);
    }

    [Fact]
    public void CalculateFormula_WithSpecificationExample_ShouldProduceExact29412()
    {
        // Arrange
        var store = new TestCurrentStore();
        var user = new TestCurrentUser();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var context = new ApplicationDbContext(options, store, user);
        var service = new PricingService(context, store, user);

        decimal actualCost = 25000m;
        decimal targetMargin = 15.0m;
        int agingDays = 10;

        // Act
        var result = service.CalculateFormula(actualCost, targetMargin, agingDays);

        // Assert: 25,000 / (1 - 0.15) = 25,000 / 0.85 = 29,411.7647... -> 29,412 THB
        Assert.Equal(29412m, result.RecommendedPrice);
        Assert.Equal(4412m, result.EstimatedGrossProfit);
        Assert.Equal(15.0m, result.EstimatedMarginPercentage);
        Assert.Equal(0.0m, result.AgingDiscountPercentage);
        Assert.Equal("0–30 วัน (ใหม่)", result.AgingBucket);
    }

    [Fact]
    public void CalculateFormula_WithAgingOver60Days_ShouldApplyDiscount()
    {
        // Arrange
        var store = new TestCurrentStore();
        var user = new TestCurrentUser();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var context = new ApplicationDbContext(options, store, user);
        var service = new PricingService(context, store, user);

        decimal actualCost = 25000m;
        decimal targetMargin = 15.0m;
        int agingDays = 75; // 61-90 days bucket (5% discount)

        // Act
        var result = service.CalculateFormula(actualCost, targetMargin, agingDays);

        // Base = 29,411.76... discounted by 5% = 27,941.176... -> 27,941 THB
        Assert.Equal(5.0m, result.AgingDiscountPercentage);
        Assert.Equal("61–90 วัน (เริ่มนาน)", result.AgingBucket);
        Assert.Equal(27941m, result.RecommendedPrice);
        Assert.True(result.RecommendedPrice >= result.ActualCost);
    }

    [Fact]
    public async Task AddCostItem_ShouldRecalculateActualCostAndTrackMovement()
    {
        // Arrange
        var (context, service, _, item) = SetupTestEnvironment();

        // Act: Add repair cost 1,200 THB
        var costDto = await service.AddCostItemAsync(item.Id, new AddCostItemRequest(
            CostType.Repair,
            1200m,
            "เปลี่ยนยางกริ๊ปและทำความสะอาดปุ่มชัตเตอร์",
            DateTimeOffset.UtcNow
        ));

        // Assert
        Assert.NotNull(costDto);
        Assert.Equal(1200m, costDto.Amount);
        Assert.Equal(CostType.Repair, costDto.CostType);

        var updatedItem = await context.InventoryItems.FindAsync(item.Id);
        Assert.NotNull(updatedItem);
        Assert.Equal(26200m, updatedItem.ActualCost); // 25,000 + 1,200

        var movements = await context.StockMovements
            .Where(sm => sm.InventoryItemId == item.Id && sm.MovementType == StockMovementType.CostAdjustment)
            .ToListAsync();
        Assert.NotEmpty(movements);

        // Cost breakdown verification
        var breakdown = await service.GetCostBreakdownAsync(item.Id);
        Assert.Equal(1200m, breakdown.RepairCost);
        Assert.Equal(26200m, breakdown.TotalActualCost);
    }

    [Fact]
    public async Task UpdatePrice_ShouldRecordPriceHistoryAudit()
    {
        // Arrange
        var (context, service, _, item) = SetupTestEnvironment();

        // Act: Update price from 30,000 to 28,500
        var historyDto = await service.UpdatePriceAsync(item.Id, new UpdatePriceRequest(
            28500m,
            "ปรับลดราคาตามสต็อก 30 วัน"
        ));

        // Assert
        Assert.NotNull(historyDto);
        Assert.Equal(30000m, historyDto.OldPrice);
        Assert.Equal(28500m, historyDto.NewPrice);
        Assert.Equal(-1500m, historyDto.PriceDifference);

        var updatedItem = await context.InventoryItems.FindAsync(item.Id);
        Assert.NotNull(updatedItem);
        Assert.Equal(28500m, updatedItem.SellingPrice);

        var histories = await service.GetPriceHistoriesAsync(item.Id);
        Assert.Single(histories);
        Assert.Equal("ปรับลดราคาตามสต็อก 30 วัน", histories[0].Reason);
    }
}
