using Mue2MueDee.Application.Common.Interfaces;

namespace Mue2MueDee.Infrastructure.Storage;

/// <summary>
/// เก็บไฟล์ไว้ใน App_Data/uploads (ใช้ตอน Dev เท่านั้น — บน Render ไฟล์จะหายเมื่อ redeploy)
/// </summary>
public class LocalImageStorage : IImageStorage
{
    private readonly string _rootPath;

    public LocalImageStorage(string contentRootPath)
    {
        _rootPath = Path.Combine(contentRootPath, "App_Data", "uploads");
    }

    public async Task<StoredImageResult> UploadAsync(Stream content, string fileName, string? folder = null, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_rootPath);

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(_rootPath, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await content.CopyToAsync(stream, ct);
        }

        return new StoredImageResult($"/api/uploads/{uniqueFileName}", null);
    }
}
