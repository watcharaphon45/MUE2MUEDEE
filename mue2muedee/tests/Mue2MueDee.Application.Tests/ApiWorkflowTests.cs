using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Mue2MueDee.Application;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;
using Mue2MueDee.Infrastructure.Identity;
using Mue2MueDee.Infrastructure.Persistence;
using Mue2MueDee.Infrastructure.Security;
using Mue2MueDee.Infrastructure.Storage;
using Mue2MueDee.Web.Controllers;
using Mue2MueDee.Web.Configuration;
using Mue2MueDee.Web.Middleware;

namespace Mue2MueDee.Application.Tests;

public class ApiWorkflowTests
{
    [Fact]
    public async Task PurchaseInventorySaleAndSupportingApis_WorkThroughHttpWithRealJwt()
    {
        // This host deliberately avoids Program.cs and its real-database startup seeding.
        const string secret = "Mue2MueDeeAuditTestsOnlySigningKey1234567890!";
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing", ApplicationName = typeof(AuthController).Assembly.FullName
        });
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = secret, ["Jwt:Issuer"] = "AuditTests", ["Jwt:Audience"] = "AuditTests"
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddApplication();
        var databaseName = Guid.NewGuid().ToString();
        builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(databaseName));
        builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentStore, CurrentStore>();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();
        builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
        builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
        builder.Services.AddSingleton<IImageStorage>(new LocalImageStorage(Path.GetTempPath()));
        builder.Services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly)
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .ConfigureApiBehaviorOptions(ApiResponseDefaults.ConfigureModelValidation);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
        {
            ApiResponseDefaults.ConfigureAuthenticationResponses(o);
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateIssuer = true, ValidIssuer = "AuditTests", ValidateAudience = true,
                ValidAudience = "AuditTests", ValidateLifetime = true, ClockSkew = TimeSpan.Zero
            };
        });
        builder.Services.AddAuthorization();
        await using var app = builder.Build();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        var store = new Store { Name = "Audit shop", Code = "AUDIT" };
        var brand = new Brand { StoreId = store.Id, Name = "Sony" };
        var category = new Category { StoreId = store.Id, Name = "Camera", Type = CategoryType.Camera };
        var grade = new ConditionGrade { StoreId = store.Id, GradeCode = "A", GradeName = "Good" };
        var product = new Product { StoreId = store.Id, Brand = brand, Category = category, ModelName = "A7", ProductCode = "A7" };
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Stores.Add(store);
            db.Products.Add(product);
            db.ConditionGrades.Add(grade);
            db.UserStores.Add(new UserStore
            {
                Store = store, Role = StoreRole.Owner,
                User = new User { Username = "audit", Email = "audit@example.test", FullName = "Audit Owner",
                    PasswordHash = new PasswordHasher().HashPassword("test-password") }
            });
            db.SaveChanges();
        }

        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };

        async Task<JsonElement> Request(HttpMethod method, string url, object? body = null)
        {
            using var request = new HttpRequestMessage(method, url);
            if (body != null) request.Content = JsonContent.Create(body);
            using var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            Assert.True(response.IsSuccessStatusCode, $"{method} {url}: {response.StatusCode}: {content}");
            using var json = JsonDocument.Parse(content);
            Assert.True(json.RootElement.GetProperty("success").GetBoolean());
            Assert.Equal((int)response.StatusCode, json.RootElement.GetProperty("statusCode").GetInt32());
            return json.RootElement.GetProperty("data").Clone();
        }

        using (var unauthorized = await client.GetAsync("/api/inventory"))
        {
            Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
            var error = await unauthorized.Content.ReadFromJsonAsync<JsonElement>();
            Assert.False(error.GetProperty("success").GetBoolean());
            Assert.Equal(401, error.GetProperty("statusCode").GetInt32());
        }
        var login = await Request(HttpMethod.Post, "/api/auth/login", new { usernameOrEmail = "audit", password = "test-password" });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.GetProperty("accessToken").GetString());
        using (var invalidModel = await client.PostAsJsonAsync("/api/customers", new { customerType = "Both", phone = "0800000000" }))
        {
            Assert.Equal(HttpStatusCode.BadRequest, invalidModel.StatusCode);
            var error = await invalidModel.Content.ReadFromJsonAsync<JsonElement>();
            Assert.False(error.GetProperty("success").GetBoolean());
            Assert.Equal(400, error.GetProperty("statusCode").GetInt32());
        }
        await Request(HttpMethod.Get, "/api/auth/me");
        await Request(HttpMethod.Get, "/api/stores/current");
        await Request(HttpMethod.Get, "/api/master-data/brands");
        await Request(HttpMethod.Get, "/api/master-data/categories");
        await Request(HttpMethod.Get, "/api/master-data/condition-grades");
        await Request(HttpMethod.Get, "/api/master-data/products");

        (string Route, object Create, object Update)[] masterCases =
        [
            ("/api/master-data/brands", new { name = "Temporary brand" }, new { name = "Updated brand", isActive = true }),
            ("/api/master-data/categories", new { name = "Temporary category", type = "Camera" },
                new { name = "Updated category", type = "Lens", isActive = true }),
            ("/api/master-data/condition-grades", new { gradeCode = "B", gradeName = "Temporary grade", sortOrder = 2 },
                new { gradeCode = "B", gradeName = "Updated grade", sortOrder = 2, isActive = true }),
            ("/api/master-data/products", new { brandId = brand.Id, categoryId = category.Id, modelName = "Temporary model", productCode = "TEMP", defaultSellingPrice = 1000 },
                new { brandId = brand.Id, categoryId = category.Id, modelName = "Updated model", productCode = "TEMP", defaultSellingPrice = 1200, isActive = true }),
            ("/api/customers", new { customerType = "Seller", name = "Temporary customer", phone = "0810000000" },
                new { customerType = "Both", name = "Updated customer", phone = "0810000000", isActive = true })
        ];
        foreach (var testCase in masterCases)
        {
            var created = await Request(HttpMethod.Post, testCase.Route, testCase.Create);
            var id = created.GetProperty("id").GetGuid();
            await Request(HttpMethod.Get, $"{testCase.Route}/{id}");
            await Request(HttpMethod.Put, $"{testCase.Route}/{id}", testCase.Update);
            await Request(HttpMethod.Delete, $"{testCase.Route}/{id}");
        }

        var customer = await Request(HttpMethod.Post, "/api/customers", new { customerType = "Both", name = "Audit Customer", phone = "0800000000" });
        var customerId = customer.GetProperty("id").GetGuid();
        var po = await Request(HttpMethod.Post, "/api/purchases", new
        {
            customerId, initialItems = Enumerable.Range(1, 2).Select(i => new
            {
                productId = product.Id, conditionGradeId = grade.Id, serialNumber = $"AUDIT-{i}",
                expectedSalePrice = 10000, estimatedRepairCost = 100, estimatedCleaningCost = 50
            })
        });
        var poId = po.GetProperty("id").GetGuid();
        var offered = await Request(HttpMethod.Post, $"/api/purchases/{poId}/offer", new { totalOfferedPrice = 18000 });
        Assert.True(offered.GetProperty("needsApproval").GetBoolean());
        await Request(HttpMethod.Post, $"/api/purchases/{poId}/approve", new { }); // JWT OWNER must pass controller policy.
        var accepted = await Request(HttpMethod.Post, $"/api/purchases/{poId}/accept", new { });
        Assert.Equal("Accepted", accepted.GetProperty("status").GetString());
        Assert.Equal(18000m, accepted.GetProperty("items").EnumerateArray().Sum(i => i.GetProperty("agreedPrice").GetDecimal()));
        var partial = await Request(HttpMethod.Post, $"/api/purchases/{poId}/payments", new { paymentMethod = "Cash", amount = 4000 });
        Assert.Equal("Accepted", partial.GetProperty("status").GetString());
        var received = await Request(HttpMethod.Post, $"/api/purchases/{poId}/receive", new { });
        Assert.Equal("Received", received.GetProperty("status").GetString());
        Assert.Equal(2, received.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("generatedSku").GetString()).Distinct().Count());
        var paid = await Request(HttpMethod.Post, $"/api/purchases/{poId}/payments", new { paymentMethod = "BankTransfer", amount = 14000 });
        Assert.Equal("Received", paid.GetProperty("status").GetString());
        await Request(HttpMethod.Get, "/api/purchases");

        var inventory = await Request(HttpMethod.Get, "/api/inventory");
        Assert.Equal(2, inventory.GetProperty("totalCount").GetInt32());
        var itemId = received.GetProperty("items")[0].GetProperty("inventoryItemId").GetGuid();
        var costs = await Request(HttpMethod.Get, $"/api/inventory/{itemId}/costs");
        Assert.Equal(100m, costs.GetProperty("repairCost").GetDecimal());
        var addedCost = await Request(HttpMethod.Post, $"/api/inventory/{itemId}/costs", new { costType = "Shipping", amount = 25 });
        await Request(HttpMethod.Delete, $"/api/inventory/{itemId}/costs/{addedCost.GetProperty("id").GetGuid()}");
        await Request(HttpMethod.Get, $"/api/inventory/{itemId}/pricing-recommendation");
        await Request(HttpMethod.Put, $"/api/inventory/{itemId}/price", new { newPrice = 11000, reason = "Audit price change" });
        var priceHistory = await Request(HttpMethod.Get, $"/api/inventory/{itemId}/price-history");
        Assert.Equal(1, priceHistory.GetArrayLength());
        await Request(HttpMethod.Get, "/api/pricing/formula-preview?actualCost=9150&targetMarginPercent=15&agingDays=1");
        await Request(HttpMethod.Post, $"/api/sales/reserve/{itemId}", new { customerName = "Audit Customer" });
        await Request(HttpMethod.Post, $"/api/sales/release-reservation/{itemId}", new { reason = "Proceed to sale" });
        var sale = await Request(HttpMethod.Post, "/api/sales", new
        {
            customerId, items = new[] { new { inventoryItemId = itemId, unitPrice = 10000, discount = 0 } },
            payments = new[] { new { paymentMethod = "Cash", amount = 10000 } }
        });
        Assert.Equal("Completed", sale.GetProperty("status").GetString());
        var soldItem = await Request(HttpMethod.Get, $"/api/inventory/{itemId}");
        Assert.Equal("Sold", soldItem.GetProperty("status").GetString());
        using (var invalidStatus = await client.PostAsJsonAsync($"/api/inventory/{itemId}/status", new { newStatus = "Available" }))
            Assert.Equal(HttpStatusCode.BadRequest, invalidStatus.StatusCode);
        var saleId = sale.GetProperty("id").GetGuid();
        await Request(HttpMethod.Post, $"/api/sales/{saleId}/cancel", new { reason = "Audit cancellation" });
        var returned = await Request(HttpMethod.Get, $"/api/inventory/{itemId}");
        Assert.Equal("Available", returned.GetProperty("status").GetString());
        await Request(HttpMethod.Get, "/api/sales");
        await Request(HttpMethod.Get, "/api/sales/trace/AUDIT-1");
        await Request(HttpMethod.Get, "/api/analytics/dashboard");
        await Request(HttpMethod.Get, "/api/analytics/stock-aging");
        await Request(HttpMethod.Get, "/api/analytics/sales-report");
        await Request(HttpMethod.Get, "/api/sticker-templates");
        var sticker = await Request(HttpMethod.Post, "/api/sticker-templates", new { name = "Audit sticker" });
        var stickerId = sticker.GetProperty("id").GetGuid();
        await Request(HttpMethod.Get, $"/api/sticker-templates/{stickerId}");
        await Request(HttpMethod.Put, $"/api/sticker-templates/{stickerId}", new { name = "Updated sticker", isDefault = true });
        var duplicate = await Request(HttpMethod.Post, $"/api/sticker-templates/{stickerId}/duplicate", new { });
        var duplicateId = duplicate.GetProperty("id").GetGuid();
        await Request(HttpMethod.Put, $"/api/sticker-templates/{duplicateId}/default", new { });
        await Request(HttpMethod.Delete, $"/api/sticker-templates/{duplicateId}");
        Assert.Equal(stickerId, (await Request(HttpMethod.Get, "/api/sticker-templates/default")).GetProperty("id").GetGuid());
        await Request(HttpMethod.Get, "/api/catalog/AUDIT");
        await Request(HttpMethod.Get, "/api/catalog/AUDIT/items");
        await Request(HttpMethod.Get, $"/api/catalog/AUDIT/items/{itemId}");
        using (var csv = await client.GetAsync("/api/analytics/sales-report/export-csv"))
            Assert.Equal(HttpStatusCode.OK, csv.StatusCode);
        using (var invalidUpload = await client.PostAsync("/api/uploads/image", new MultipartFormDataContent()))
            Assert.Equal(HttpStatusCode.BadRequest, invalidUpload.StatusCode);
        using (var forbidden = await client.GetAsync("/api/super-admin/stores"))
        {
            Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
            var error = await forbidden.Content.ReadFromJsonAsync<JsonElement>();
            Assert.False(error.GetProperty("success").GetBoolean());
            Assert.Equal(403, error.GetProperty("statusCode").GetInt32());
        }
        await app.StopAsync();
    }
}
