using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;
using Mue2MueDee.Infrastructure.Persistence;
using Mue2MueDee.Infrastructure.Security;

namespace Mue2MueDee.Application.Tests;

public class StoreSecurityTests
{
    [Fact]
    public async Task RegistrationRejectsDuplicateStoreCodeAndReportsConfiguredTokenExpiry()
    {
        var store = new TestCurrentStore();
        var user = new TestCurrentUser();
        using var context = CreateContext(store, user);
        context.Stores.Add(new Store { Code = "TAKEN", Name = "Existing" });
        context.SaveChanges();
        var tokens = new JwtTokenService(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["Jwt:ExpiryMinutes"] = "30" }).Build());
        var service = new AuthService(context, new PasswordHasher(), tokens, user, store);
        var request = new RegisterRequest("new@example.test", "newuser", "test-password", "New Owner", null, "New shop", "TAKEN");
        var error = await Assert.ThrowsAsync<AppException>(() => service.RegisterStoreWithOwnerAsync(request));
        Assert.Equal(409, error.StatusCode);
        Assert.Empty(context.Users);
        var result = await service.RegisterStoreWithOwnerAsync(request with { StoreCode = "NEW" });
        Assert.InRange((result.ExpiresAt - DateTimeOffset.UtcNow).TotalMinutes, 29.9, 30.1);
    }

    [Fact]
    public void SuperAdminWithStoreRole_RetainsOwnerPermissionsInCurrentStore()
    {
        var accessor = new Microsoft.AspNetCore.Http.HttpContextAccessor
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                [
                    new(System.Security.Claims.ClaimTypes.Role, "SUPERADMIN"),
                    new(System.Security.Claims.ClaimTypes.Role, "OWNER")
                ], "test"))
            }
        };
        Assert.Equal(StoreRole.Owner, new Mue2MueDee.Infrastructure.Identity.CurrentStore(accessor).Role);
    }

    [Fact]
    public async Task AdminCannotCreateOwnerMembership()
    {
        var store = new TestCurrentStore { Role = StoreRole.Admin };
        var currentUser = new TestCurrentUser();
        using var context = CreateContext(store, currentUser);
        var service = new StoreService(context, store, currentUser, new PasswordHasher());

        var error = await Assert.ThrowsAsync<AppException>(() => service.AddUserToStoreAsync(
            new AddUserToStoreRequest("owner@test.com", "Owner", null, "temporary", StoreRole.Owner)));

        Assert.Equal(403, error.StatusCode);
        Assert.Empty(context.UserStores);
        Assert.Empty(context.Users);
    }

    [Theory]
    [InlineData(StoreRole.Staff, true)]
    [InlineData(StoreRole.Owner, false)]
    public async Task CannotRemoveLastActiveOwner(StoreRole role, bool active)
    {
        var store = new TestCurrentStore();
        var currentUser = new TestCurrentUser();
        using var context = CreateContext(store, currentUser);
        var owner = new User();
        currentUser.UserId = owner.Id;
        var storeEntity = new Store();
        store.StoreId = storeEntity.Id;
        context.UserStores.Add(new UserStore
        {
            Store = storeEntity, User = owner,
            Role = StoreRole.Owner, IsActive = true
        });
        context.SaveChanges();
        var service = new StoreService(context, store, currentUser, new PasswordHasher());

        var error = await Assert.ThrowsAsync<AppException>(() => service.UpdateUserStoreRoleAsync(
            owner.Id, new UpdateUserStoreRoleRequest(role, active)));

        Assert.Equal(400, error.StatusCode);
        Assert.Equal(StoreRole.Owner, (await context.UserStores.SingleAsync()).Role);
    }

    [Fact]
    public async Task DisabledUserCannotRefreshToken()
    {
        var store = new TestCurrentStore();
        var currentUser = new TestCurrentUser();
        using var context = CreateContext(store, currentUser);
        context.RefreshTokens.Add(new RefreshToken
        {
            User = new User { IsActive = false }, TokenHash = "test-refresh",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
        });
        context.SaveChanges();
        var service = new AuthService(context, new PasswordHasher(),
            new JwtTokenService(new ConfigurationBuilder().Build()), currentUser, store);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RefreshTokenAsync(
            new RefreshTokenRequest("test-refresh")));
        Assert.Single(context.RefreshTokens);
    }

    private static ApplicationDbContext CreateContext(TestCurrentStore store, TestCurrentUser user) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, store, user);
}
