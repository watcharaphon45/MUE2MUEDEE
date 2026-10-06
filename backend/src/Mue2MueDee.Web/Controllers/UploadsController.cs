using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.Common.Exceptions;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadsController : ApiControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UploadsController> _logger;

    public UploadsController(IWebHostEnvironment environment, ILogger<UploadsController> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    [HttpPost("image")]
    [Authorize]
    public async Task<IActionResult> UploadImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            throw AppException.BadRequest("กรุณาเลือกไฟล์รูปภาพที่ต้องการอัปโหลด");
        }

        if (file.Length > 10 * 1024 * 1024) // 10MB
        {
            throw AppException.BadRequest("ขนาดไฟล์รูปภาพต้องไม่เกิน 10MB");
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        if (!allowedExtensions.Contains(ext))
        {
            throw AppException.BadRequest("รองรับเฉพาะไฟล์รูปภาพนามสกุล .jpg, .jpeg, .png, .webp, .gif เท่านั้น");
        }

        var uploadsFolder = Path.Combine(_environment.ContentRootPath, "App_Data", "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("Image uploaded successfully: {FileName} ({Size} bytes)", uniqueFileName, file.Length);

        var url = $"/api/uploads/{uniqueFileName}";
        return Success(new { Url = url, FileName = file.FileName, Size = file.Length }, "อัปโหลดรูปภาพสำเร็จ");
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
