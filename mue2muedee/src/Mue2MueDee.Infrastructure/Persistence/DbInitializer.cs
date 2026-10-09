using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;

namespace Mue2MueDee.Infrastructure.Persistence;

public class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher, ILogger logger)
    {
        try
        {
            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
            }

            if (!await context.Stores.AnyAsync())
            {
                logger.LogInformation("Seeding initial store and owner user...");

                var defaultStore = new Store
                {
                    Name = "ABC Camera — ร้านกล้องและเลนส์มือสอง",
                    Code = "ABCCAM",
                    Phone = "081-234-5678",
                    Email = "contact@abccamera.com",
                    Address = "123 ถนนสุขุมวิท แขวงคลองเตย เขตคลองเตย กรุงเทพฯ 10110",
                    TaxId = "0105550000000",
                    Facebook = "https://facebook.com/abccamera",
                    Line = "@abccamera",
                    PrimaryColor = "#4f46e5",
                    LogoUrl = "/logo.png",
                    IsActive = true
                };

                var defaultOwner = new User
                {
                    Email = "owner@abccamera.com",
                    Username = "owner",
                    FullName = "คุณเจ้าของร้าน มือสองมือดี",
                    PhoneNumber = "081-234-5678",
                    PasswordHash = passwordHasher.HashPassword("Admin1234!"),
                    IsActive = true,
                    IsSuperAdmin = true
                };

                var defaultStaff = new User
                {
                    Email = "staff@abccamera.com",
                    Username = "staff",
                    FullName = "พนักงานหน้าร้าน",
                    PhoneNumber = "089-876-5432",
                    PasswordHash = passwordHasher.HashPassword("Staff1234!"),
                    IsActive = true,
                    IsSuperAdmin = false
                };

                var defaultSuperAdmin = new User
                {
                    Email = "superadmin@mue2muedee.com",
                    Username = "superadmin",
                    FullName = "ผู้ดูแลระบบสูงสุด (Super Admin)",
                    PhoneNumber = "080-000-0000",
                    PasswordHash = passwordHasher.HashPassword("Admin1234!"),
                    IsActive = true,
                    IsSuperAdmin = true
                };

                var ownerUserStore = new UserStore
                {
                    User = defaultOwner,
                    Store = defaultStore,
                    Role = StoreRole.Owner,
                    IsActive = true
                };

                var staffUserStore = new UserStore
                {
                    User = defaultStaff,
                    Store = defaultStore,
                    Role = StoreRole.Staff,
                    IsActive = true
                };

                var superAdminUserStore = new UserStore
                {
                    User = defaultSuperAdmin,
                    Store = defaultStore,
                    Role = StoreRole.Owner,
                    IsActive = true
                };

                context.Stores.Add(defaultStore);
                context.Users.AddRange(defaultOwner, defaultStaff, defaultSuperAdmin);
                context.UserStores.AddRange(ownerUserStore, staffUserStore, superAdminUserStore);

                await context.SaveChangesAsync();
                logger.LogInformation("Default store and users seeded successfully.");
            }
            else
            {
                // Ensure existing owner or superadmin is marked as SuperAdmin
                var ownerUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "owner");
                if (ownerUser != null && !ownerUser.IsSuperAdmin)
                {
                    ownerUser.IsSuperAdmin = true;
                }

                if (!await context.Users.AnyAsync(u => u.Username == "superadmin"))
                {
                    var firstStore = await context.Stores.FirstOrDefaultAsync();
                    var superAdminUser = new User
                    {
                        Email = "superadmin@mue2muedee.com",
                        Username = "superadmin",
                        FullName = "ผู้ดูแลระบบสูงสุด (Super Admin)",
                        PhoneNumber = "080-000-0000",
                        PasswordHash = passwordHasher.HashPassword("Admin1234!"),
                        IsActive = true,
                        IsSuperAdmin = true
                    };
                    context.Users.Add(superAdminUser);

                    if (firstStore != null)
                    {
                        context.UserStores.Add(new UserStore
                        {
                            User = superAdminUser,
                            Store = firstStore,
                            Role = StoreRole.Owner,
                            IsActive = true
                        });
                    }
                }

                await context.SaveChangesAsync();
            }

            // Seed default sticker templates for stores that do not have any yet
            var stores = await context.Stores.ToListAsync();
            foreach (var st in stores)
            {
                var hasTemplates = await context.StickerTemplates.IgnoreQueryFilters().AnyAsync(t => t.StoreId == st.Id);
                if (!hasTemplates)
                {
                    logger.LogInformation("Seeding default sticker templates for store: {StoreName}", st.Name);
                    context.StickerTemplates.AddRange(StickerTemplateService.CreateDefaultTemplates(st.Id, st.Name, st.LogoUrl));
                }
            }
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        }
    }
}
