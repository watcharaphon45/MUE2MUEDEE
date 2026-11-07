using Microsoft.EntityFrameworkCore;

namespace Mue2MueDee.Application.Common;

internal static class SequentialCode
{
    // Callers scope the query to a store and include deleted records so codes are never reused.
    public static async Task<int> NextAsync(IQueryable<string> codes, string prefix)
    {
        var existing = await codes.Where(code => code.StartsWith(prefix)).ToListAsync();
        return existing.Select(code => int.TryParse(code[prefix.Length..], out var value) ? value : 0)
            .DefaultIfEmpty(0).Max() + 1;
    }
}
