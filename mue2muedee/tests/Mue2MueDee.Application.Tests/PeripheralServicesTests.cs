using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;
using Mue2MueDee.Infrastructure.Persistence;
using Mue2MueDee.Infrastructure.Security;
using Mue2MueDee.Infrastructure.Storage;

namespace Mue2MueDee.Application.Tests;

public class PeripheralServicesTests
{
    [Fact]
    public async Task MasterData_WithPurchaseHistory_CannotBeDeletedBeforeReceiving()
    {
        var store = new TestCurrentStore();
        var storeId = store.StoreId!.Value;
        using var context = CreateContext(store);
        var customer = new Customer { StoreId = storeId, Name = "Historical seller" };
        var grade = new ConditionGrade { StoreId = storeId, GradeCode = "A", GradeName = "Good" };
        var product = new Product
        {
            StoreId = storeId, ModelName = "A7", ProductCode = "A7",
            Brand = new Brand { StoreId = storeId, Name = "Sony" },
            Category = new Category { StoreId = storeId, Name = "Camera" }
        };
        var po = new PurchaseOrder { StoreId = storeId, PoNumber = "HISTORY", Customer = customer };
        po.Items.Add(new PurchaseItem
        {
            StoreId = storeId, Product = product, ConditionGrade = grade, SerialNumber = "HISTORY-1"
        });
        context.PurchaseOrders.Add(po);
        context.SaveChanges();
        var service = new MasterDataService(context, store);
        await Assert.ThrowsAsync<AppException>(() => service.DeleteCustomerAsync(customer.Id));
        await Assert.ThrowsAsync<AppException>(() => service.DeleteProductAsync(product.Id));
        await Assert.ThrowsAsync<AppException>(() => service.DeleteConditionGradeAsync(grade.Id));
        Assert.Single(context.PurchaseOrders);
        Assert.Single(context.Products);
    }

    private static ApplicationDbContext CreateContext(TestCurrentStore store) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, store, new TestCurrentUser());

    [Fact]
    public async Task PublicCatalog_OnlyReturnsAvailableUndeletedItemsForRequestedShop()
    {
        var currentStore = new TestCurrentStore();
        using var context = CreateContext(currentStore);
        var publicStore = new Store { Code = "PUBLIC", Name = "Public shop" };
        var product = new Product
        {
            StoreId = publicStore.Id, ModelName = "A7", ProductCode = "A7",
            Brand = new Brand { StoreId = publicStore.Id, Name = "Sony" },
            Category = new Category { StoreId = publicStore.Id, Name = "Camera" }
        };
        var grade = new ConditionGrade { StoreId = publicStore.Id, GradeName = "Good", GradeCode = "A" };
        context.Stores.Add(publicStore);
        context.Products.Add(product);
        context.ConditionGrades.Add(grade);
        context.InventoryItems.AddRange(
            new InventoryItem { StoreId = publicStore.Id, Product = product, ConditionGrade = grade,
                Sku = "PUBLIC-AVAILABLE", SerialNumber = "1", Status = InventoryStatus.Available,
                Notes = "Private seller information and purchase cost" },
            new InventoryItem { StoreId = publicStore.Id, Product = product, ConditionGrade = grade,
                Sku = "PUBLIC-SOLD", SerialNumber = "2", Status = InventoryStatus.Sold },
            new InventoryItem { StoreId = publicStore.Id, Product = product, ConditionGrade = grade,
                Sku = "PUBLIC-DELETED", SerialNumber = "3", Status = InventoryStatus.Available, IsDeleted = true });
        context.SaveChanges();
        var service = new CatalogService(context);
        var result = await service.GetCatalogByStoreCodeAsync("public", new CatalogQueryParameters());
        Assert.Single(result.Items);
        Assert.Equal("PUBLIC-AVAILABLE", result.Items[0].Sku);
        Assert.Null(result.Items[0].Notes);
        Assert.Null((await service.GetCatalogItemDetailAsync("PUBLIC", result.Items[0].Id)).Notes);
        Assert.Equal(1, result.FilterMeta.TotalAvailableItems);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCatalogItemDetailAsync("PUBLIC",
            context.InventoryItems.IgnoreQueryFilters().Single(i => i.Sku == "PUBLIC-SOLD").Id));
    }

    [Fact]
    public async Task StickerTemplates_RejectForeignStoreAccessAndKeepSingleDefault()
    {
        var store = new TestCurrentStore();
        using var context = CreateContext(store);
        var foreign = new StickerTemplate { StoreId = Guid.NewGuid(), Name = "Other shop" };
        context.StickerTemplates.Add(foreign);
        context.SaveChanges();
        var service = new StickerTemplateService(context, store);
        var error = await Assert.ThrowsAsync<AppException>(() => service.GetByIdAsync(foreign.Id));
        Assert.Equal(404, error.StatusCode);
        var first = await service.CreateAsync(new CreateStickerTemplateDto { Name = "First" });
        var second = await service.CreateAsync(new CreateStickerTemplateDto { Name = "Second", IsDefault = true });
        Assert.True(first.IsDefault);
        Assert.Equal(second.Id, (await service.GetDefaultAsync())!.Id);
        Assert.Single(await service.GetAllAsync(), t => t.IsDefault);
        await service.DeleteAsync(second.Id);
        Assert.Equal(first.Id, (await service.GetDefaultAsync())!.Id);
    }

    [Fact]
    public async Task StickerTemplates_RejectNegativeMarginsAndOversizedSheets()
    {
        var store = new TestCurrentStore();
        using var context = CreateContext(store);
        var service = new StickerTemplateService(context, store);
        await Assert.ThrowsAsync<AppException>(() => service.CreateAsync(
            new CreateStickerTemplateDto { Name = "Negative", MarginLeftMm = -10m }));
        await Assert.ThrowsAsync<AppException>(() => service.CreateAsync(
            new CreateStickerTemplateDto { Name = "Too tall", IsContinuousRoll = false,
                PaperHeightMm = 50m, RowsCount = 3, LabelHeightMm = 30m }));
        Assert.Empty(context.StickerTemplates);
    }

    [Fact]
    public async Task SuperAdmin_DeniesRegularStoreUsers()
    {
        using var context = CreateContext(new TestCurrentStore());
        var service = new SuperAdminService(context, new PasswordHasher(), new TestCurrentUser());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetStoresAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetStatsAsync());
    }

    [Fact]
    public async Task LocalImageStorage_WritesUniqueFilesAtUrlsServedByUploadsController()
    {
        var root = Path.Combine(Path.GetTempPath(), "mue2muedee-audit-" + Guid.NewGuid().ToString("N"));
        var files = new List<string>();
        try
        {
            var service = new LocalImageStorage(root);
            using var content = new MemoryStream([1, 2, 3]);
            var first = await service.UploadAsync(content, "photo.PNG");
            content.Position = 0;
            var second = await service.UploadAsync(content, "photo.PNG");
            Assert.NotEqual(first.Url, second.Url);
            foreach (var result in new[] { first, second })
            {
                Assert.StartsWith("/api/uploads/", result.Url);
                var path = Path.Combine(root, "App_Data", "uploads", Path.GetFileName(result.Url));
                files.Add(path);
                Assert.Equal(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(path));
            }
        }
        finally
        {
            foreach (var path in files) File.Delete(path);
            var uploads = Path.Combine(root, "App_Data", "uploads");
            if (Directory.Exists(uploads)) Directory.Delete(uploads);
            var data = Path.Combine(root, "App_Data");
            if (Directory.Exists(data)) Directory.Delete(data);
            if (Directory.Exists(root)) Directory.Delete(root);
        }
    }
}
