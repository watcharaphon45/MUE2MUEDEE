using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;
using Mue2MueDee.Infrastructure.Persistence;

namespace Mue2MueDee.Application.Tests;

public class TestCurrentStore : ICurrentStore
{
    public Guid? StoreId { get; set; } = Guid.NewGuid();
    public StoreRole? Role { get; set; } = StoreRole.Owner;
    public bool HasStoreContext => StoreId.HasValue;
}

public class TestCurrentUser : ICurrentUser
{
    public Guid? UserId { get; set; } = Guid.NewGuid();
    public string? Username { get; set; } = "tester";
    public string? Email { get; set; } = "tester@mue2muedee.com";
    public bool IsAuthenticated => true;
    public bool IsSuperAdmin { get; set; } = false;
}

public class InventoryServiceTests
{
    private (ApplicationDbContext Context, InventoryService Service, Guid StoreId, Guid ProductId, Guid GradeId) SetupTestEnvironment()
    {
        var store = new TestCurrentStore();
        var user = new TestCurrentUser();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options, store, user);

        var brand = new Brand { StoreId = store.StoreId!.Value, Name = "Sony" };
        var category = new Category { StoreId = store.StoreId!.Value, Name = "Mirrorless Camera", Type = CategoryType.Camera };
        var grade = new ConditionGrade { StoreId = store.StoreId!.Value, GradeCode = "A+", GradeName = "สภาพนางฟ้า", SortOrder = 1 };
        var product = new Product
        {
            StoreId = store.StoreId!.Value,
            BrandId = brand.Id,
            CategoryId = category.Id,
            ModelName = "Alpha 7 IV (ILCE-7M4)",
            ProductCode = "SONY-A7M4",
            DefaultSellingPrice = 65000m
        };

        context.Brands.Add(brand);
        context.Categories.Add(category);
        context.ConditionGrades.Add(grade);
        context.Products.Add(product);
        context.SaveChanges();

        var service = new InventoryService(context, store, user);
        return (context, service, store.StoreId!.Value, product.Id, grade.Id);
    }

    [Fact]
    public async Task CreateInventoryItemAsync_ShouldAutoGenerateSkuAndRecordMovement()
    {
        var (context, service, storeId, productId, gradeId) = SetupTestEnvironment();

        var request = new CreateInventoryItemRequest(
            ProductId: productId,
            SerialNumber: "SN-987654321",
            ConditionGradeId: gradeId,
            PurchaseCost: 45000m,
            ActualCost: 45000m,
            SellingPrice: 59000m,
            ShutterCount: 3500,
            Accessories: "ครบกล่อง คู่มือ สายคล้อง แบตแท้ 1 ก้อน",
            Defects: "ไม่มีตำหนิ สภาพสวยมาก",
            Notes: "ซื้อจากคุณเอ",
            CustomerId: null,
            ReceivedAt: null
        );

        var item = await service.CreateInventoryItemAsync(request);

        Assert.NotNull(item);
        Assert.StartsWith("CAM-", item.Sku);
        Assert.Equal("SN-987654321", item.SerialNumber);
        Assert.Equal(45000m, item.PurchaseCost);
        Assert.Equal(45000m, item.ActualCost);
        Assert.Equal(59000m, item.SellingPrice);
        Assert.Equal(14000m, item.ExpectedProfit);
        Assert.Equal(InventoryStatus.Available, item.Status);

        var movements = await service.GetStockMovementsAsync(item.Id);
        Assert.Single(movements);
        Assert.Equal(StockMovementType.Receive, movements[0].MovementType);
    }

    [Fact]
    public async Task CreateInventoryItemAsync_WithDuplicateSerial_ShouldThrowConflict()
    {
        var (context, service, storeId, productId, gradeId) = SetupTestEnvironment();

        var request = new CreateInventoryItemRequest(
            ProductId: productId,
            SerialNumber: "SN-DUPLICATE-01",
            ConditionGradeId: gradeId,
            PurchaseCost: 40000m,
            ActualCost: null,
            SellingPrice: 50000m,
            ShutterCount: null,
            Accessories: null,
            Defects: null,
            Notes: null,
            CustomerId: null,
            ReceivedAt: null
        );

        await service.CreateInventoryItemAsync(request);

        // Try creating duplicate serial for the same product
        var exception = await Assert.ThrowsAsync<AppException>(() => service.CreateInventoryItemAsync(request));
        Assert.Equal(409, exception.StatusCode);
        Assert.Contains("มีอยู่ในระบบของร้านแล้ว", exception.Message);
    }

    [Fact]
    public async Task AdjustCostAsync_ShouldIncreaseActualCostAndLogMovement()
    {
        var (context, service, storeId, productId, gradeId) = SetupTestEnvironment();

        var created = await service.CreateInventoryItemAsync(new CreateInventoryItemRequest(
            ProductId: productId,
            SerialNumber: "SN-COST-ADJUST",
            ConditionGradeId: gradeId,
            PurchaseCost: 30000m,
            ActualCost: 30000m,
            SellingPrice: 42000m,
            ShutterCount: null,
            Accessories: null,
            Defects: null,
            Notes: null,
            CustomerId: null,
            ReceivedAt: null
        ));

        // Adjust cost: e.g. sensor cleaning + 1,200 THB
        var updated = await service.AdjustCostAsync(created.Id, new AdjustInventoryCostRequest(
            AdditionalCost: 1200m,
            CostTypeReason: "ค่าล้างเซนเซอร์และทำความสะอาดช่องมองภาพ",
            Notes: "ส่งช่างภายนอก"
        ));

        Assert.Equal(31200m, updated.ActualCost);
        Assert.Equal(42000m - 31200m, updated.ExpectedProfit);

        var movements = await service.GetStockMovementsAsync(created.Id);
        Assert.Equal(2, movements.Count);
        Assert.Contains(movements, m => m.MovementType == StockMovementType.CostAdjustment);
    }

    [Fact]
    public async Task UpdateStatusAsync_ToSold_ShouldSetSoldAtAndRecordMovement()
    {
        var (context, service, storeId, productId, gradeId) = SetupTestEnvironment();

        var created = await service.CreateInventoryItemAsync(new CreateInventoryItemRequest(
            ProductId: productId,
            SerialNumber: "SN-STATUS-TEST",
            ConditionGradeId: gradeId,
            PurchaseCost: 20000m,
            ActualCost: 20000m,
            SellingPrice: 28000m,
            ShutterCount: null,
            Accessories: null,
            Defects: null,
            Notes: null,
            CustomerId: null,
            ReceivedAt: null
        ));

        var updated = await service.UpdateStatusAsync(created.Id, new UpdateInventoryStatusRequest(
            NewStatus: InventoryStatus.Sold,
            Reason: "ขายหน้าร้าน",
            ReferenceNumber: "INV-2026-0001"
        ));

        Assert.Equal(InventoryStatus.Sold, updated.Status);
        Assert.NotNull(updated.SoldAt);

        var movements = await service.GetStockMovementsAsync(created.Id);
        Assert.Contains(movements, m => m.MovementType == StockMovementType.Sell && m.Quantity == -1);
    }
}
