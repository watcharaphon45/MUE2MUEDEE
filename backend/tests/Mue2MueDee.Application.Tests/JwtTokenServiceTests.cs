using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Domain.Enums;
using Mue2MueDee.Infrastructure.Security;
using Xunit;

namespace Mue2MueDee.Application.Tests;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _jwtService;

    public JwtTokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "SuperSecretKeyForTestingJwtTokens1234567890!@#$",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

        _jwtService = new JwtTokenService(config);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainStoreIdAndRoleClaims()
    {
        // Arrange
        var user = new User
        {
            Email = "owner@test.com",
            Username = "owner_test",
            FullName = "Store Owner"
        };
        var storeId = Guid.NewGuid();
        var role = StoreRole.Owner;

        // Act
        var token = _jwtService.GenerateAccessToken(user, storeId, role);

        // Assert
        Assert.NotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Equal(storeId.ToString(), jwt.Claims.First(c => c.Type == "store_id").Value);
        Assert.Equal("OWNER", jwt.Claims.First(c => c.Type == "role").Value);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnRandomBase64String()
    {
        // Act
        var token1 = _jwtService.GenerateRefreshToken();
        var token2 = _jwtService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(token1);
        Assert.NotNull(token2);
        Assert.NotEqual(token1, token2);
    }
}
