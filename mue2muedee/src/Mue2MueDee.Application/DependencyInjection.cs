using Microsoft.Extensions.DependencyInjection;
using Mue2MueDee.Application.Services;

namespace Mue2MueDee.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IMasterDataService, MasterDataService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ISuperAdminService, SuperAdminService>();
        services.AddScoped<ICatalogService, CatalogService>();

        return services;
    }
}
