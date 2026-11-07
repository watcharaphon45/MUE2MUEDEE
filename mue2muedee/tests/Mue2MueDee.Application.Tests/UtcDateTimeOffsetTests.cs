using Microsoft.EntityFrameworkCore;
using Mue2MueDee.Domain.Entities;
using Mue2MueDee.Infrastructure.Persistence;

namespace Mue2MueDee.Application.Tests;

public class UtcDateTimeOffsetTests
{
    private static ApplicationDbContext CreateContext()
    {
        // Model and SQL generation only; these tests never open a database connection.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=utc_conversion_tests;Username=unused;Password=unused")
            .UseSnakeCaseNamingConvention()
            .Options;
        return new ApplicationDbContext(options, new TestCurrentStore(), new TestCurrentUser());
    }

    [Theory]
    [InlineData(7)]
    [InlineData(-5)]
    [InlineData(0)]
    public void EveryTimestamp_NormalizesWritesWithoutChangingTheInstant(int offsetHours)
    {
        using var context = CreateContext();
        var local = new DateTimeOffset(2026, 11, 7, 10, 30, 0, TimeSpan.FromHours(offsetHours));
        var properties = context.Model.GetEntityTypes()
            .SelectMany(entity => entity.GetProperties())
            .Where(property => (Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType)
                == typeof(DateTimeOffset))
            .ToList();

        Assert.NotEmpty(properties);
        Assert.Contains(properties, property => property.ClrType == typeof(DateTimeOffset?));

        foreach (var property in properties)
        {
            var mapping = property.GetRelationalTypeMapping();
            Assert.Equal("timestamp with time zone", mapping.StoreType);
            var converter = mapping.Converter;
            Assert.NotNull(converter);
            var stored = Assert.IsType<DateTimeOffset>(converter.ConvertToProvider(local));
            Assert.Equal(TimeSpan.Zero, stored.Offset);
            Assert.Equal(local.UtcTicks, stored.UtcTicks);
            // Use the actual PostgreSQL mapping to exercise Npgsql's UTC validation.
            Assert.NotEmpty(mapping.GenerateSqlLiteral(local));

            var loaded = Assert.IsType<DateTimeOffset>(converter.ConvertFromProvider(stored));
            Assert.Equal(TimeSpan.Zero, loaded.Offset);
            Assert.Equal(local.UtcTicks, loaded.UtcTicks);
            if (property.IsNullable)
            {
                Assert.Null(converter.ConvertToProvider(null));
                Assert.Null(converter.ConvertFromProvider(null));
            }
        }
    }

    [Fact]
    public void DateFilters_ConvertThaiMidnightToPreviousUtcDay()
    {
        using var context = CreateContext();
        var start = new DateTimeOffset(2026, 11, 7, 0, 0, 0, TimeSpan.FromHours(7));
        var end = start.AddDays(1);
        var queries = new[]
        {
            context.SaleOrders.Where(order => order.SaleDate >= start && order.SaleDate < end).ToQueryString(),
            context.PurchaseOrders.Where(order => order.PurchaseDate >= start && order.PurchaseDate < end).ToQueryString(),
            context.InventoryItems.Where(item => item.ReceivedAt >= start && item.ReceivedAt < end).ToQueryString(),
            context.Users.Where(user => user.UpdatedAt >= start && user.UpdatedAt < end).ToQueryString()
        };

        foreach (var sql in queries)
        {
            Assert.DoesNotContain("+07:00", sql);
            Assert.Contains("2026-11-06T17:00:00", sql);
            Assert.Contains("2026-11-07T17:00:00", sql);
        }
    }
}
