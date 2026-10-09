using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Application.Common.Interfaces;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadsController : ApiControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UploadsController> _logger;
    private readonly IImageStorage _imageStorage;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public UploadsController(IWebHostEnvironment environment, ILogger<UploadsController> logger, IImageStorage imageStorage)
    {
        _environment = environment;
        _logger = logger;
        _imageStorage = imageStorage;
    }

    [HttpPost("image")]
    [Authorize]
    public async Task<IActionResult> UploadImage(IFormFile? file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            throw AppException.BadRequest("กรุณาเลือกไฟล์รูปภาพที่ต้องการอัปโหลด");
        }

        var result = await SaveAsync(file, ct);
        return Success(result, "อัปโหลดรูปภาพสำเร็จ");
    }

    [HttpPost("images")]
    [Authorize]
    [RequestSizeLimit(200 * 1024 * 1024)]
    public async Task<IActionResult> UploadImages(List<IFormFile>? files, CancellationToken ct)
    {
        if (files == null || files.Count == 0)
        {
            throw AppException.BadRequest("กรุณาเลือกไฟล์รูปภาพอย่างน้อย 1 ไฟล์");
        }

        var results = new List<object>();
        foreach (var file in files.Where(f => f.Length > 0))
        {
            results.Add(await SaveAsync(file, ct));
        }

        return Success(results, $"อัปโหลดรูปภาพสำเร็จ {results.Count} รูป");
    }

    private async Task<object> SaveAsync(IFormFile file, CancellationToken ct)
    {
        if (file.Length > MaxFileSize)
        {
            throw AppException.BadRequest($"ไฟล์ '{file.FileName}' มีขนาดเกิน 10MB");
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            throw AppException.BadRequest($"ไฟล์ '{file.FileName}' ไม่รองรับ — รองรับเฉพาะ .jpg, .jpeg, .png, .webp, .gif");
        }

        await using var stream = file.OpenReadStream();
        var stored = await _imageStorage.UploadAsync(stream, file.FileName, "items", ct);

        _logger.LogInformation("Image uploaded: {Url} ({Size} bytes)", stored.Url, file.Length);

        return new { Url = stored.Url, FileName = file.FileName, Size = file.Length };
    }

    [HttpGet("{fileName}")]
    [AllowAnonymous]
    public IActionResult GetImage(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        var filePath = Path.Combine(_environment.ContentRootPath, "App_Data", "uploads", safeFileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var ext = Path.GetExtension(safeFileName).ToLowerInvariant();
        var contentType = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };

        return PhysicalFile(filePath, contentType);
    }
}
