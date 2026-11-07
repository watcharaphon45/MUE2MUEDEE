using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;
using Mue2MueDee.Infrastructure.Persistence;
using Mue2MueDee.Infrastructure.Security;

namespace Mue2MueDee.Application.Tests;

public class SuperAdminAccessTests
{
    private static (ApplicationDbContext Db, AuthService Auth, TestCurrentStore CurrentStore, User Admin, Store First, Store Second, Store Disabled) Setup()
    {
        var currentStore = new TestCurrentStore { StoreId = null, Role = null };
        var currentUser = new TestCurrentUser { IsSuperAdmin = true };
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, currentStore, currentUser);
        var hasher = new PasswordHasher();
        var admin = new User { Username = "platform.admin", Email = "platform@example.test", FullName = "Platform admin",
            IsSuperAdmin = true, PasswordHash = hasher.HashPassword("test-password") };
        var first = new Store { Name = "First", Code = "FIRST" };
        var second = new Store { Name = "Second", Code = "SECOND" };
        var disabled = new Store { Name = "Disabled", Code = "DISABLED", IsActive = false };
        db.Users.Add(admin);
        db.Stores.AddRange(first, second, disabled);
        db.SaveChanges();
        currentUser.UserId = admin.Id;
        var auth = new AuthService(db, hasher, new JwtTokenService(new ConfigurationBuilder().Build()), currentUser, currentStore);
        return (db, auth, currentStore, admin, first, second, disabled);
    }

    [Fact]
    public async Task PlatformLoginListsActiveStoresWithoutSelectingOneOrCreatingMemberships()
    {
        var setup = Setup();
        using var db = setup.Db;
        var result = await setup.Auth.LoginAsync(new LoginRequest("platform.admin", "test-password"));
        Assert.Null(result.CurrentStoreId);
        Assert.Null(result.CurrentRole);
        Assert.Equal(2, result.AvailableStores.Count);
        Assert.All(result.AvailableStores, s => Assert.Equal(StoreRole.Owner, s.Role));
        Assert.DoesNotContain(result.AvailableStores, s => s.StoreId == setup.Disabled.Id);
        var claims = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken).Claims;
        Assert.Contains(claims, c => c.Value == "SUPERADMIN");
        Assert.DoesNotContain(claims, c => c.Type == "store_id");
        Assert.Empty(db.UserStores);
    }

    [Fact]
    public async Task SwitchingAndRefreshingPreservesSelectedStoreAndIncludesNewStores()
    {
        var setup = Setup();
        using var db = setup.Db;
        var result = await setup.Auth.SwitchStoreAsync(new SwitchStoreRequest(setup.Second.Id));
        Assert.Equal(setup.Second.Id, result.CurrentStoreId);
        Assert.Equal(StoreRole.Owner, result.CurrentRole);
        var claims = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken).Claims;
        Assert.Contains(claims, c => c.Type == "store_id" && c.Value == setup.Second.Id.ToString());
        Assert.Contains(claims, c => c.Value == "SUPERADMIN");
        Assert.Contains(claims, c => c.Value == "OWNER");
        setup.CurrentStore.StoreId = setup.Second.Id;
        db.Stores.Add(new Store { Name = "New", Code = "NEW" });
        await db.SaveChangesAsync();
        var refreshed = await setup.Auth.RefreshTokenAsync(new RefreshTokenRequest(result.RefreshToken));
        Assert.Equal(setup.Second.Id, refreshed.CurrentStoreId);
        Assert.Equal(3, refreshed.AvailableStores.Count);
        Assert.Empty(db.UserStores);
    }

    [Fact]
    public async Task RefreshingPlatformSessionDoesNotSelectAStoreAutomatically()
    {
        var setup = Setup();
        using var db = setup.Db;
        var login = await setup.Auth.LoginAsync(new LoginRequest("platform.admin", "test-password"));
        var result = await setup.Auth.RefreshTokenAsync(new RefreshTokenRequest(login.RefreshToken));
        Assert.Null(result.CurrentStoreId);
        Assert.Equal(2, result.AvailableStores.Count);
    }

    [Fact]
    public async Task DisabledOrMissingStoreCannotBeSelectedEvenBySuperAdmin()
    {
        var setup = Setup();
        using var db = setup.Db;
        foreach (var id in new[] { setup.Disabled.Id, Guid.NewGuid() })
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => setup.Auth.SwitchStoreAsync(new SwitchStoreRequest(id)));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => setup.Auth.LoginAsync(new LoginRequest("platform.admin", "test-password", id)));
        }
        Assert.Empty(db.RefreshTokens);
    }

    [Fact]
    public async Task OrdinaryUserCanOnlySelectActiveMembershipsEvenIfCurrentTokenSaysSuperAdmin()
    {
        var setup = Setup();
        using var db = setup.Db;
        setup.Admin.IsSuperAdmin = false;
        db.UserStores.Add(new UserStore { User = setup.Admin, Store = setup.First, Role = StoreRole.Staff });
        await db.SaveChangesAsync();
        var login = await setup.Auth.LoginAsync(new LoginRequest("platform.admin", "test-password"));
        Assert.Equal(setup.First.Id, login.CurrentStoreId);
        Assert.Equal(StoreRole.Staff, login.CurrentRole);
        Assert.Single(login.AvailableStores);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => setup.Auth.SwitchStoreAsync(new SwitchStoreRequest(setup.Second.Id)));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => setup.Auth.LoginAsync(new LoginRequest("platform.admin", "test-password", setup.Second.Id)));
    }

    [Fact]
    public async Task SelectedStoreKeepsTenantQueryIsolation()
    {
        var setup = Setup();
        using var db = setup.Db;
        db.InventoryItems.AddRange(
            new InventoryItem { StoreId = setup.First.Id, Sku = "FIRST-001", SerialNumber = "FIRST-SN" },
            new InventoryItem { StoreId = setup.Second.Id, Sku = "SECOND-001", SerialNumber = "SECOND-SN" });
        await db.SaveChangesAsync();
        var result = await setup.Auth.SwitchStoreAsync(new SwitchStoreRequest(setup.Second.Id));
        setup.CurrentStore.StoreId = result.CurrentStoreId;
        var visible = await db.InventoryItems.ToListAsync();
        Assert.Single(visible);
        Assert.Equal("SECOND-001", visible[0].Sku);
    }
}
