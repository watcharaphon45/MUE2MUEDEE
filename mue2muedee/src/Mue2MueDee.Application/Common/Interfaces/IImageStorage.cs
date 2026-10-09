namespace Mue2MueDee.Application.Common.Interfaces;

public record StoredImageResult(string Url, string? PublicId);

/// <summary>
/// ที่เก็บไฟล์รูปภาพ (Cloudinary บน Production / Local disk สำหรับ Dev)
/// </summary>
public interface IImageStorage
{
    Task<StoredImageResult> UploadAsync(Stream content, string fileName, string? folder = null, CancellationToken ct = default);
}
