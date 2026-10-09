using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.Common.Interfaces;

namespace Mue2MueDee.Infrastructure.Storage;

public class CloudinaryImageStorage : IImageStorage
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryImageStorage(string cloudinaryUrl)
    {
        _cloudinary = new Cloudinary(cloudinaryUrl);
        _cloudinary.Api.Secure = true;
    }

    public async Task<StoredImageResult> UploadAsync(Stream content, string fileName, string? folder = null, CancellationToken ct = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, content),
            Folder = string.IsNullOrWhiteSpace(folder) ? "mue2muedee" : $"mue2muedee/{folder}",
            UseFilename = false,
            UniqueFilename = true,
            Overwrite = false,
            // ย่อรูปไม่ให้เกิน 1600px และบีบอัดอัตโนมัติ เพื่อประหยัดพื้นที่/โหลดไว
            Transformation = new Transformation().Width(1600).Height(1600).Crop("limit").Quality("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams, ct);

        if (result.Error != null)
            throw AppException.BadRequest($"อัปโหลดรูปภาพไปยัง Cloudinary ไม่สำเร็จ: {result.Error.Message}");

        // f_auto = ส่ง WebP/AVIF ตาม browser อัตโนมัติ
        var url = result.SecureUrl.ToString().Replace("/image/upload/", "/image/upload/f_auto,q_auto/");
        return new StoredImageResult(url, result.PublicId);
    }
}
