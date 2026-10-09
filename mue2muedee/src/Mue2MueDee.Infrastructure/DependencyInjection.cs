using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Infrastructure.Identity;
using Mue2MueDee.Infrastructure.Persistence;
using Mue2MueDee.Infrastructure.Security;
using Mue2MueDee.Infrastructure.Storage;

namespace Mue2MueDee.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Port=5432;Database=mue2muedee;Username=postgres;Password=postgres";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, b =>
            {
                b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            })
            .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // HTTP Context Accessor & Tenant Context
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ICurrentStore, CurrentStore>();

        // Security Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // Image Storage: ใช้ Cloudinary ถ้ามีการตั้งค่า CLOUDINARY_URL (cloudinary://key:secret@cloud_name)
        var cloudinaryUrl = configuration["Cloudinary:Url"] ?? configuration["CLOUDINARY_URL"];
        if (!string.IsNullOrWhiteSpace(cloudinaryUrl))
        {
            services.AddSingleton<IImageStorage>(_ => new CloudinaryImageStorage(cloudinaryUrl));
        }
        else
        {
            services.AddSingleton<IImageStorage>(sp =>
                new LocalImageStorage(sp.GetRequiredService<IHostEnvironment>().ContentRootPath));
        }

        return services;
    }
}
