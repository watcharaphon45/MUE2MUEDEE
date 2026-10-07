using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.Common.Models;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

[Authorize(Roles = "SUPERADMIN")]
[Route("api/super-admin")]
public class SuperAdminController : ApiControllerBase
{
    private readonly ISuperAdminService _superAdminService;

    public SuperAdminController(ISuperAdminService superAdminService)
    {
        _superAdminService = superAdminService;
    }

    [HttpGet("stores")]
    [ProducesResponseType(typeof(ApiResponse<List<SuperAdminStoreListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStores(CancellationToken ct)
    {
        var stores = await _superAdminService.GetStoresAsync(ct);
        return Success(stores, "ดึงรายชื่อร้านค้าทั้งหมดสำเร็จ");
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<SuperAdminStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var stats = await _superAdminService.GetStatsAsync(ct);
        return Success(stats, "ดึงข้อมูลสถิติภาพรวมสำเร็จ");
    }

    [HttpPost("stores")]
    [ProducesResponseType(typeof(ApiResponse<SuperAdminStoreListItemDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateStoreWithOwner([FromBody] CreateStoreWithOwnerRequest request, CancellationToken ct)
    {
        var created = await _superAdminService.CreateStoreWithOwnerAsync(request, ct);
        return CreatedSuccess(created, $"สร้างร้านค้า '{created.Name}' และบัญชีเจ้าของร้านเรียบร้อยแล้ว");
    }

    [HttpPut("stores/{id:guid}/toggle-status")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleStoreStatus([FromRoute] Guid id, CancellationToken ct)
    {
        var isActive = await _superAdminService.ToggleStoreStatusAsync(id, ct);
        var msg = isActive ? "เปิดการใช้งานร้านค้าเรียบร้อยแล้ว" : "ระงับการใช้งานร้านค้าเรียบร้อยแล้ว";
        return Success(isActive, msg);
    }
}
