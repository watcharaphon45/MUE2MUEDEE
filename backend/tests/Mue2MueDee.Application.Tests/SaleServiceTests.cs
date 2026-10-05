using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;
using Mue2MueDee.Infrastructure.Persistence;
using Xunit;

namespace Mue2MueDee.Application.Tests;

public class SaleServiceTests
{
    private (ApplicationDbContext Context, SaleService Service, Guid StoreId, InventoryItem Item, Customer Customer) SetupTestEnvironment()
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
            Code = "ABC"
        };
        store.StoreId = storeEntity.Id;
        context.Stores.Add(storeEntity);

        var brand = new Brand { StoreId = store.StoreId!.Value, Name = "Sony" };
        var category = new Category { StoreId = store.StoreId!.Value, Name = "Camera", Type = CategoryType.Camera };
        var grade = new ConditionGrade { StoreId = store.StoreId!.Value, GradeCode = "A", GradeName = "สภาพดีมาก", SortOrder = 1 };
        var customer = new Customer
        {
            StoreId = store.StoreId!.Value,
            Name = "สมชาย ลูกค้าประจำ",
            Phone = "081-999-8888",
            CustomerType = CustomerType.Both
        };

        var product = new Product
        {
            StoreId = store.StoreId!.Value,
            BrandId = brand.Id,
            CategoryId = category.Id,
            ModelName = "Sony A7 III",
            ProductCode = "SONY-A7M3",
            DefaultSellingPrice = 30000m
        };

        context.Brands.Add(brand);
        context.Categories.Add(category);
        context.ConditionGrades.Add(grade);
        context.Customers.Add(customer);
        context.Products.Add(product);

        // Product with ActualCost = 25,000 THB
        var item = new InventoryItem
        {
            StoreId = store.StoreId!.Value,
            Sku = "CAM-2610-0099",
            ProductId = product.Id,
            SerialNumber = "SN-A7M3-9999",
            ConditionGradeId = grade.Id,
            Status = InventoryStatus.Available,
            PurchaseCost = 24000m,
            ActualCost = 25000m,
            SellingPrice = 29000m,
            CustomerId = customer.Id,
            ReceivedAt = DateTimeOffset.UtcNow.AddDays(-5)
        };
        context.InventoryItems.Add(item);
        context.SaveChanges();

        var service = new SaleService(context, store, user);
        return (context, service, store.StoreId!.Value, item, customer);
    }

    [Fact]
    public async Task CreateSale_ShouldCalculateGrossProfitAndMarginExactlyAsSpec()
    {
        // Specification Example (Section 24):
        // Selling Price: 29,000 THB
        // Actual Cost:   25,000 THB
        // Gross Profit:   4,000 THB
        // Margin:         4,000 / 29,000 * 100 = 13.79%

        // Arrange
        var (context, service, _, item, customer) = SetupTestEnvironment();

        var request = new CreateSaleOrderRequest(
            CustomerId: customer.Id,
            CustomerName: customer.Name,
            CustomerPhone: customer.Phone,
            Notes: "ขายหน้าร้าน",
            Items: new List<CreateSaleItemRequest>
            {
                new CreateSaleItemRequest(item.Id, 29000m, 0m, 30)
            },
            Payments: new List<CreateSalePaymentRequest>
            {
                new CreateSalePaymentRequest(PaymentMethod.PromptPay, 29000m, "PP-12345", "ชำระเต็มจำนวน")
            }
        );

        // Act
        var result = await service.CreateSaleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("SO-", result.SaleNumber);
        Assert.Equal(29000m, result.TotalAmount);
        Assert.Equal(29000m, result.NetAmount);
        Assert.Equal(25000m, result.TotalCost);
        Assert.Equal(4000m, result.TotalGrossProfit);
        Assert.Equal(13.79m, result.TotalMarginPercentage);
        Assert.Single(result.Items);
        Assert.Equal(4000m, result.Items[0].GrossProfit);
        Assert.Equal(13.79m, result.Items[0].MarginPercentage);

        // Check InventoryItem updated to Sold
        var updatedItem = await context.InventoryItems.FindAsync(item.Id);
        Assert.NotNull(updatedItem);
        Assert.Equal(InventoryStatus.Sold, updatedItem.Status);
        Assert.NotNull(updatedItem.SoldAt);

        // Check StockMovement created
        var movement = await context.StockMovements
            .FirstOrDefaultAsync(m => m.InventoryItemId == item.Id && m.MovementType == StockMovementType.Sell);
        Assert.NotNull(movement);
        Assert.Equal(-1, movement.Quantity);
    }

    [Fact]
    public async Task CreateSale_WhenItemAlreadySold_ShouldThrowConflictException()
    {
        // Arrange
        var (context, service, _, item, customer) = SetupTestEnvironment();
        item.Status = InventoryStatus.Sold;
        await context.SaveChangesAsync();

        var request = new CreateSaleOrderRequest(
            CustomerId: customer.Id,
            CustomerName: customer.Name,
            CustomerPhone: customer.Phone,
            Notes: null,
            Items: new List<CreateSaleItemRequest>
            {
                new CreateSaleItemRequest(item.Id, 29000m, 0m, 30)
            },
            Payments: new List<CreateSalePaymentRequest>
            {
                new CreateSalePaymentRequest(PaymentMethod.Cash, 29000m, null, null)
            }
        );

        // Act & Assert: Must throw Conflict
        await Assert.ThrowsAsync<AppException>(() => service.CreateSaleAsync(request));
    }

    [Fact]
    public async Task CancelSale_ShouldRevertItemStatusAndAddReturnMovement()
    {
        // Arrange
        var (context, service, _, item, customer) = SetupTestEnvironment();

        var sale = await service.CreateSaleAsync(new CreateSaleOrderRequest(
            CustomerId: customer.Id,
            CustomerName: customer.Name,
            CustomerPhone: customer.Phone,
            Notes: null,
            Items: new List<CreateSaleItemRequest>
            {
                new CreateSaleItemRequest(item.Id, 29000m, 0m, 30)
            },
            Payments: new List<CreateSalePaymentRequest>
            {
                new CreateSalePaymentRequest(PaymentMethod.Cash, 29000m, null, null)
            }
        ));

        // Act: Cancel sale
        var cancelled = await service.CancelSaleAsync(sale.Id, "ลูกค้าขอยกเลิกและเปลี่ยนรุ่น");

        // Assert
        Assert.Equal(SaleStatus.Cancelled, cancelled.Status);

        var restoredItem = await context.InventoryItems.FindAsync(item.Id);
        Assert.NotNull(restoredItem);
        Assert.Equal(InventoryStatus.Available, restoredItem.Status);
        Assert.Null(restoredItem.SoldAt);

        var returnMovement = await context.StockMovements
            .FirstOrDefaultAsync(m => m.InventoryItemId == item.Id && m.MovementType == StockMovementType.Return);
        Assert.NotNull(returnMovement);
        Assert.Equal(1, returnMovement.Quantity);
    }

    [Fact]
    public async Task GetSerialTrace_ShouldReturnFullLifecycleHistory()
    {
        // Arrange
        var (_, service, _, item, customer) = SetupTestEnvironment();

        await service.CreateSaleAsync(new CreateSaleOrderRequest(
            CustomerId: customer.Id,
            CustomerName: "ลูกค้าซื้อ คุณวิชัย",
            CustomerPhone: "089-111-2222",
            Notes: null,
            Items: new List<CreateSaleItemRequest>
            {
                new CreateSaleItemRequest(item.Id, 29000m, 0m, 30)
            },
            Payments: new List<CreateSalePaymentRequest>
            {
                new CreateSalePaymentRequest(PaymentMethod.BankTransfer, 29000m, "REF-8899", null)
            }
        ));

        // Act
        var trace = await service.GetSerialTraceAsync(item.SerialNumber);

        // Assert
        Assert.NotNull(trace);
        Assert.Equal(item.SerialNumber, trace.SerialNumber);
        Assert.Equal(item.Sku, trace.Sku);
        Assert.Equal("Sony A7 III", trace.ProductModelName);
        Assert.Equal(24000m, trace.PurchaseCost);
        Assert.Equal(25000m, trace.ActualCost);
        Assert.Equal("ลูกค้าซื้อ คุณวิชัย", trace.SoldToCustomer);
        Assert.Equal(29000m, trace.SoldPrice);
        Assert.Equal(4000m, trace.GrossProfit);
        Assert.Equal(13.79m, trace.MarginPercentage);
    }
}
