using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

[Authorize]
[Route("api/sticker-templates")]
public class StickerTemplatesController : ApiControllerBase
{
    private readonly IStickerTemplateService _service;

    public StickerTemplatesController(IStickerTemplateService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAllAsync();
        return Success(list, "ดึงรายการรูปแบบสติ๊กเกอร์สำเร็จ");
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        return Success(item, "ดึงข้อมูลรูปแบบสติ๊กเกอร์สำเร็จ");
    }

    [HttpGet("default")]
    public async Task<IActionResult> GetDefault()
    {
        var item = await _service.GetDefaultAsync();
        return Success(item, "ดึงรูปแบบสติ๊กเกอร์เริ่มต้นสำเร็จ");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStickerTemplateDto dto)
    {
        var item = await _service.CreateAsync(dto);
        return CreatedSuccess(item, $"สร้างรูปแบบสติ๊กเกอร์ '{item.Name}' สำเร็จ");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStickerTemplateDto dto)
    {
        var item = await _service.UpdateAsync(id, dto);
        return Success(item, $"แก้ไขรูปแบบสติ๊กเกอร์ '{item.Name}' สำเร็จ");
    }

    [HttpPost("{id:guid}/duplicate")]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        var item = await _service.DuplicateAsync(id);
        return CreatedSuccess(item, $"คัดลอกรูปแบบสติ๊กเกอร์เป็น '{item.Name}' สำเร็จ");
    }

    [HttpPut("{id:guid}/default")]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        await _service.SetDefaultAsync(id);
        return Success("ตั้งเป็นรูปแบบสติ๊กเกอร์เริ่มต้นสำเร็จ");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Success("ลบรูปแบบสติ๊กเกอร์สำเร็จ");
    }

    [HttpPost("reset-defaults")]
    public async Task<IActionResult> ResetDefaults()
    {
        await _service.ResetToSystemDefaultsAsync();
        return Success("รีเซ็ตเป็นรูปแบบมาตรฐานระบบสำเร็จ");
    }
}
