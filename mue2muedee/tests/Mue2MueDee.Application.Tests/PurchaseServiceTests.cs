using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;
using Mue2MueDee.Infrastructure.Persistence;

namespace Mue2MueDee.Application.Tests;

public class PurchaseServiceTests
{
    private (ApplicationDbContext Context, PurchaseService Service, Guid StoreId, Guid ProductId, Guid GradeId, Guid CustomerId) SetupTestEnvironment()
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
            DefaultSellingPrice = 30000m
        };
        var customer = new Customer
        {
            StoreId = store.StoreId!.Value,
            Name = "คุณสมชาย กล้องมือสอง",
            Phone = "0812345678",
            CustomerType = CustomerType.Seller
        };

        context.Brands.Add(brand);
        context.Categories.Add(category);
        context.ConditionGrades.Add(grade);
        context.Products.Add(product);
        context.Customers.Add(customer);
        context.SaveChanges();

        var service = new PurchaseService(context, store, user);
        return (context, service, store.StoreId!.Value, product.Id, grade.Id, customer.Id);
    }

    [Fact]
    public void CalculateMaxBuyPrice_ShouldMatchSpecificationFormula()
    {
        // Spec Example:
        // ExpectedSale = 30,000, TargetMargin = 15% (30,000 * 0.85 = 25,500)
        // Repair = 1,000, Cleaning = 500, MissingAccessories = 0
        // Expected MaxBuy = 24,000
        var maxBuy = PurchaseService.CalculateMaxBuyPrice(
            expectedSale: 30000m,
            repair: 1000m,
            cleaning: 500m,
            missingAccessories: 0m,
            targetMargin: 0.15m
        );

        Assert.Equal(24000m, maxBuy);
    }

    [Fact]
    public async Task CreatePurchaseOrder_AndMakeOffer_ExceedingMaxBuy_ShouldSetNeedsApprovalTrue()
    {
        var (context, service, storeId, productId, gradeId, customerId) = SetupTestEnvironment();

        var po = await service.CreatePurchaseOrderAsync(new CreatePurchaseOrderRequest(
            CustomerId: customerId,
            Notes: "ลูกค้านำกล้อง Sony A7 IV มาขาย",
            InitialItems: new List<CreatePurchaseItemRequest>
            {
                new(
                    ProductId: productId,
                    SerialNumber: "SN-A7M4-PO-01",
                    ConditionGradeId: gradeId,
                    ShutterCount: 4500,
                    Accessories: "กล่อง แบต 1 ก้อน",
                    Defects: "สภาพสวย ไร้รอย",
                    ExpectedSalePrice: 30000m,
                    EstimatedRepairCost: 1000m,
                    EstimatedCleaningCost: 500m,
                    MissingAccessoriesDeduction: 0m
                )
            }
        ));

        Assert.NotNull(po);
        Assert.StartsWith("PO-", po.PoNumber);
        Assert.Equal(24000m, po.TotalMaxBuyPrice);

        // Offer higher than MaxBuy (25,000 > 24,000)
        var offered = await service.MakeOfferAsync(po.Id, new MakeOfferRequest(
            TotalOfferedPrice: 25000m,
            Notes: "ลูกค้าขอยอด 25,000 บาท ขอดึงราคาพิเศษ"
        ));

        Assert.True(offered.NeedsApproval);
        Assert.Equal(PurchaseStatus.Offered, offered.Status);

        // Approve by Owner/Admin
        var approved = await service.ApprovePurchaseAsync(po.Id);
        Assert.False(approved.NeedsApproval);
        Assert.NotNull(approved.ApprovedBy);
    }

    [Fact]
    public async Task ReceiveToStock_ShouldCreateInventoryItemWithAutoSkuAndStockMovement()
    {
        var (context, service, storeId, productId, gradeId, customerId) = SetupTestEnvironment();

        var po = await service.CreatePurchaseOrderAsync(new CreatePurchaseOrderRequest(
            CustomerId: customerId,
            Notes: "ซื้อกล้อง Sony A7 IV เข้าคลัง",
            InitialItems: new List<CreatePurchaseItemRequest>
            {
                new(
                    ProductId: productId,
                    SerialNumber: "SN-RECEIVE-TEST-99",
                    ConditionGradeId: gradeId,
                    ShutterCount: 2000,
                    Accessories: "ครบกล่อง",
                    Defects: null,
                    ExpectedSalePrice: 30000m,
                    EstimatedRepairCost: 800m,
                    EstimatedCleaningCost: 200m
                )
            }
        ));

        // Accept offer
        await service.AcceptOfferAsync(po.Id);

        // Record payment to customer (Bank Transfer)
        await service.RecordPaymentAsync(po.Id, new RecordPurchasePaymentRequest(
            PaymentMethod: PaymentMethod.BankTransfer,
            Amount: po.TotalAgreedPrice,
            ReferenceNumber: "SLIP-KBANK-8899",
            ProofUrl: null,
            Notes: "โอนผ่าน K-Plus"
        ));

        // Receive into stock
        var received = await service.ReceiveToStockAsync(po.Id);

        Assert.Equal(PurchaseStatus.Received, received.Status);
        Assert.NotNull(received.Items[0].InventoryItemId);

        // Verify Inventory Item created in DB
        var inventoryItem = await context.InventoryItems
            .Include(i => i.StockMovements)
            .FirstOrDefaultAsync(i => i.Id == received.Items[0].InventoryItemId!.Value);

        Assert.NotNull(inventoryItem);
        Assert.StartsWith("CAM-", inventoryItem.Sku);
        Assert.Equal("SN-RECEIVE-TEST-99", inventoryItem.SerialNumber);
        Assert.Equal(po.TotalAgreedPrice, inventoryItem.PurchaseCost);
        // Actual cost = AgreedPrice + repair (800) + cleaning (200)
        Assert.Equal(po.TotalAgreedPrice + 1000m, inventoryItem.ActualCost);
        Assert.Equal(30000m, inventoryItem.SellingPrice);
        Assert.Equal(InventoryStatus.Available, inventoryItem.Status);

        Assert.Single(inventoryItem.StockMovements);
        Assert.Equal(StockMovementType.Receive, inventoryItem.StockMovements.First().MovementType);
    }
}
