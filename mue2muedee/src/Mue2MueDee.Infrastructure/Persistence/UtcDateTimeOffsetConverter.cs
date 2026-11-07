using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Mue2MueDee.Infrastructure.Persistence;

/// <summary>
/// Preserves the instant while normalizing database values and parameters to UTC.
/// </summary>
public sealed class UtcDateTimeOffsetConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
{
    public UtcDateTimeOffsetConverter()
        : base(value => value.ToUniversalTime(), value => value.ToUniversalTime())
    {
    }
}
