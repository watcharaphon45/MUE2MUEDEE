using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;
using Mue2MueDee.Infrastructure.Persistence;
using Xunit;

namespace Mue2MueDee.Application.Tests;

public class AnalyticsServiceTests
{
    private (ApplicationDbContext Context, AnalyticsService Service, TestCurrentStore Store, Guid StoreId) SetupTestEnvironment(StoreRole role = StoreRole.Owner)
    {
        var store = new TestCurrentStore { Role = role };
        var user = new TestCurrentUser();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options, store, user);

        var storeEntity = new Store
        {
            Name = "MUE2MUEDEE Camera Shop",
            Code = "MMC",
            DefaultTargetMargin = 15.0m
        };
        store.StoreId = storeEntity.Id;
        context.Stores.Add(storeEntity);

        var pricingService = new PricingService(context, store, user);
        var analyticsService = new AnalyticsService(context, store, pricingService);

        return (context, analyticsService, store, storeEntity.Id);
    }

    [Fact]
    public async Task GetDashboardMetrics_CalculatesAccurateCountsAndFinancials_ForOwner()
    {
        var (context, service, _, storeId) = SetupTestEnvironment(StoreRole.Owner);

        var brand = new Brand { StoreId = storeId, Name = "Fujifilm" };
        var category = new Category { StoreId = storeId, Name = "Mirrorless", Type = CategoryType.Camera };
        var grade = new ConditionGrade { StoreId = storeId, GradeCode = "A+", GradeName = "Mint" };
        var product = new Product
        {
            StoreId = storeId,
            BrandId = brand.Id,
            CategoryId = category.Id,
            ModelName = "X-T5",
            ProductCode = "FUJI-XT5"
        };
        context.Brands.Add(brand);
        context.Categories.Add(category);
        context.ConditionGrades.Add(grade);
        context.Products.Add(product);

        // 1 item received today (Available)
        var item1 = new InventoryItem
        {
            StoreId = storeId,
            ProductId = product.Id,
            ConditionGradeId = grade.Id,
            Sku = "SKU-001",
            SerialNumber = "SN-001",
            Status = InventoryStatus.Available,
            ActualCost = 40000m,
            SellingPrice = 47000m,
            ReceivedAt = DateTimeOffset.UtcNow
        };

        // 1 item received 70 days ago (Available, Aging 61-90)
        var item2 = new InventoryItem
        {
            StoreId = storeId,
            ProductId = product.Id,
            ConditionGradeId = grade.Id,
            Sku = "SKU-002",
            SerialNumber = "SN-002",
            Status = InventoryStatus.Available,
            ActualCost = 20000m,
            SellingPrice = 24000m,
            ReceivedAt = DateTimeOffset.UtcNow.AddDays(-70)
        };

        // 1 item received 100 days ago (Available, Aging 91-120 -> Dead Capital)
        var item3 = new InventoryItem
        {
            StoreId = storeId,
            ProductId = product.Id,
            ConditionGradeId = grade.Id,
            Sku = "SKU-003",
            SerialNumber = "SN-003",
            Status = InventoryStatus.Available,
            ActualCost = 15000m,
            SellingPrice = 18000m,
            ReceivedAt = DateTimeOffset.UtcNow.AddDays(-100)
        };

        // 1 item PendingInspection
        var item4 = new InventoryItem
        {
            StoreId = storeId,
            ProductId = product.Id,
            ConditionGradeId = grade.Id,
            Sku = "SKU-004",
            Status = InventoryStatus.PendingInspection,
            ActualCost = 10000m,
            SellingPrice = 12000m,
            ReceivedAt = DateTimeOffset.UtcNow
        };

        context.InventoryItems.AddRange(item1, item2, item3, item4);

        // Add 1 Completed Sale today
        var saleOrder = new SaleOrder
        {
            StoreId = storeId,
            SaleNumber = "SO-TODAY-001",
            SaleDate = DateTimeOffset.UtcNow,
            TotalAmount = 30000m,
            DiscountAmount = 1000m,
            NetAmount = 29000m,
            TotalCost = 25000m,
            TotalGrossProfit = 4000m,
            TotalMarginPercentage = 13.79m,
            Status = SaleStatus.Completed
        };
        context.SaleOrders.Add(saleOrder);
        await context.SaveChangesAsync();

        var metrics = await service.GetDashboardMetricsAsync();

        Assert.Equal(3, metrics.AvailableItems);
        Assert.Equal(1, metrics.PendingInspection);
        Assert.Equal(4, metrics.TotalActiveStockCount);
        Assert.Equal(2, metrics.StockAgingOver60DaysCount); // item2 (70d) and item3 (100d)
        Assert.Equal(1, metrics.StockAgingOver90DaysCount); // item3 (100d)
        Assert.Equal(29000m, metrics.SalesTodayAmount);
        Assert.Equal(1, metrics.SalesTodayCount);

        // Owner sees financials
        Assert.True(metrics.IsFinancialVisible);
        Assert.Equal(85000m, metrics.InventoryCostValue); // 40k + 20k + 15k + 10k
        Assert.Equal(101000m, metrics.InventorySalesValue); // 47k + 24k + 18k + 12k
        Assert.Equal(15000m, metrics.DeadCapitalCost); // item3 ActualCost
        Assert.Equal(4000m, metrics.GrossProfitThisMonth);
    }

    [Fact]
    public async Task GetDashboardMetrics_MasksFinancialData_ForStaffRole()
    {
        var (context, service, _, storeId) = SetupTestEnvironment(StoreRole.Staff);

        var item = new InventoryItem
        {
            StoreId = storeId,
            Sku = "SKU-STAFF-TEST",
            Status = InventoryStatus.Available,
            ActualCost = 25000m,
            SellingPrice = 30000m,
            ReceivedAt = DateTimeOffset.UtcNow
        };
        context.InventoryItems.Add(item);
        await context.SaveChangesAsync();

        var metrics = await service.GetDashboardMetricsAsync();

        Assert.False(metrics.IsFinancialVisible);
        Assert.Null(metrics.InventoryCostValue);
        Assert.Null(metrics.GrossProfitThisMonth);
        Assert.Null(metrics.DeadCapitalCost);
        Assert.Null(metrics.AverageProfitMarginThisMonth);
        // Operational counts remain visible
        Assert.Equal(1, metrics.AvailableItems);
    }

    [Fact]
    public async Task GetStockAgingReport_CategorizesBucketsAndCalculatesClearancePrice()
    {
        var (context, service, _, storeId) = SetupTestEnvironment(StoreRole.Owner);

        var brand = new Brand { StoreId = storeId, Name = "Canon" };
        var category = new Category { StoreId = storeId, Name = "Lens", Type = CategoryType.Lens };
        var grade = new ConditionGrade { StoreId = storeId, GradeCode = "A", GradeName = "Very Good" };
        var product = new Product
        {
            StoreId = storeId,
            BrandId = brand.Id,
            CategoryId = category.Id,
            ModelName = "RF 50mm f/1.2L"
        };
        context.Brands.Add(brand);
        context.Categories.Add(category);
        context.ConditionGrades.Add(grade);
        context.Products.Add(product);

        // Item 1: 15 days (0-30 bucket)
        var item1 = new InventoryItem
        {
            StoreId = storeId,
            ProductId = product.Id,
            ConditionGradeId = grade.Id,
            Sku = "SKU-FRESH",
            Status = InventoryStatus.Available,
            ActualCost = 40000m,
            SellingPrice = 48000m,
            ReceivedAt = DateTimeOffset.UtcNow.AddDays(-15)
        };

        // Item 2: 75 days (61-90 bucket -> 5% discount)
        var item2 = new InventoryItem
        {
            StoreId = storeId,
            ProductId = product.Id,
            ConditionGradeId = grade.Id,
            Sku = "SKU-AGING",
            Status = InventoryStatus.Available,
            ActualCost = 20000m,
            SellingPrice = 24000m,
            ReceivedAt = DateTimeOffset.UtcNow.AddDays(-75)
        };

        context.InventoryItems.AddRange(item1, item2);
        await context.SaveChangesAsync();

        var report = await service.GetStockAgingReportAsync();

        Assert.Equal(2, report.TotalItemsInStock);
        Assert.Equal(5, report.Buckets.Count);

        var b030 = report.Buckets.First(b => b.BucketKey == "0_30");
        Assert.Equal(1, b030.Count);

        var b6190 = report.Buckets.First(b => b.BucketKey == "61_90");
        Assert.Equal(1, b6190.Count);
        Assert.Equal(5.0m, b6190.DiscountPercentage);

        // Check clearance price for aged item
        var agedItem = report.AgingItems.First(i => i.Sku == "SKU-AGING");
        Assert.Equal(75, agedItem.DaysInStock);
        Assert.Equal(5.0m, agedItem.AgingDiscountPercentage);
        Assert.True(agedItem.RecommendedClearancePrice < item2.SellingPrice);
    }

    [Fact]
    public async Task GetSalesReport_AggregatesBrandCategoryAndDailyTrendCorrectly()
    {
        var (context, service, _, storeId) = SetupTestEnvironment(StoreRole.Owner);

        var brand = new Brand { StoreId = storeId, Name = "Sony" };
        var category = new Category { StoreId = storeId, Name = "Camera", Type = CategoryType.Camera };
        var product = new Product
        {
            StoreId = storeId,
            BrandId = brand.Id,
            CategoryId = category.Id,
            ModelName = "A7 IV"
        };
        context.Brands.Add(brand);
        context.Categories.Add(category);
        context.Products.Add(product);

        var saleOrder = new SaleOrder
        {
            StoreId = storeId,
            SaleNumber = "SO-TEST-001",
            SaleDate = DateTimeOffset.UtcNow,
            TotalAmount = 50000m,
            DiscountAmount = 2000m,
            NetAmount = 48000m,
            TotalCost = 40000m,
            TotalGrossProfit = 8000m,
            TotalMarginPercentage = 16.67m,
            Status = SaleStatus.Completed
        };
        var saleItem = new SaleItem
        {
            StoreId = storeId,
            SaleOrderId = saleOrder.Id,
            ProductId = product.Id,
            ProductModelName = "Sony A7 IV",
            Sku = "SONY-A7M4-01",
            ActualCost = 40000m,
            UnitPrice = 50000m,
            Discount = 2000m,
            SubTotal = 48000m,
            GrossProfit = 8000m,
            MarginPercentage = 16.67m,
            Product = product
        };
        var payment = new SalePayment
        {
            StoreId = storeId,
            SaleOrderId = saleOrder.Id,
            PaymentMethod = PaymentMethod.PromptPay,
            Amount = 48000m
        };
        saleOrder.Items.Add(saleItem);
        saleOrder.Payments.Add(payment);

        context.SaleOrders.Add(saleOrder);
        await context.SaveChangesAsync();

        var report = await service.GetSalesReportAsync(new SalesReportQuery());

        Assert.Equal(1, report.Summary.OrderCount);
        Assert.Equal(1, report.Summary.ItemsSoldCount);
        Assert.Equal(48000m, report.Summary.TotalNetSales);
        Assert.Equal(8000m, report.Summary.TotalGrossProfit);

        Assert.Single(report.BrandBreakdown);
        Assert.Equal("Sony", report.BrandBreakdown[0].GroupName);
        Assert.Equal(48000m, report.BrandBreakdown[0].TotalSales);

        Assert.Single(report.PaymentBreakdown);
        Assert.Equal("พร้อมเพย์ (PromptPay)", report.PaymentBreakdown[0].MethodName);
        Assert.Equal(48000m, report.PaymentBreakdown[0].TotalAmount);
    }

    [Fact]
    public async Task ExportSalesReportCsv_ProducesValidUtf8BomBytes()
    {
        var (context, service, _, storeId) = SetupTestEnvironment(StoreRole.Owner);

        var saleOrder = new SaleOrder
        {
            StoreId = storeId,
            SaleNumber = "SO-CSV-001",
            SaleDate = DateTimeOffset.UtcNow,
            TotalAmount = 10000m,
            NetAmount = 10000m,
            TotalCost = 8000m,
            TotalGrossProfit = 2000m,
            TotalMarginPercentage = 20m,
            Status = SaleStatus.Completed
        };
        context.SaleOrders.Add(saleOrder);
        await context.SaveChangesAsync();

        var bytes = await service.ExportSalesReportCsvAsync(new SalesReportQuery());

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 3);
        // Verify UTF-8 BOM: 0xEF, 0xBB, 0xBF
        Assert.Equal(0xEF, bytes[0]);
        Assert.Equal(0xBB, bytes[1]);
        Assert.Equal(0xBF, bytes[2]);
    }
}
