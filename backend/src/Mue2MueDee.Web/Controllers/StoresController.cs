using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.Common.Models;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

[Authorize]
public class StoresController : ApiControllerBase
{
    private readonly IStoreService _storeService;

    public StoresController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<StoreDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentStore(CancellationToken ct)
    {
        var store = await _storeService.GetCurrentStoreAsync(ct);
        return Success(store, "ดึงข้อมูลร้านค้าสำเร็จ");
    }

    [Authorize(Roles = "OWNER,ADMIN")]
    [HttpPut("current")]
    [ProducesResponseType(typeof(ApiResponse<StoreDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCurrentStore([FromBody] UpdateStoreRequest request, CancellationToken ct)
    {
        var store = await _storeService.UpdateCurrentStoreAsync(request, ct);
        return Success(store, "อัปเดตข้อมูลร้านค้าเรียบร้อยแล้ว");
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<List<StoreUserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStoreUsers(CancellationToken ct)
    {
        var users = await _storeService.GetStoreUsersAsync(ct);
        return Success(users, "ดึงรายชื่อผู้ใช้ในร้านสำเร็จ");
    }

    [Authorize(Roles = "OWNER,ADMIN")]
    [HttpPost("users")]
    [ProducesResponseType(typeof(ApiResponse<StoreUserDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddUserToStore([FromBody] AddUserToStoreRequest request, CancellationToken ct)
    {
        var user = await _storeService.AddUserToStoreAsync(request, ct);
        return CreatedSuccess(user, "เพิ่มผู้ใช้เข้าร้านค้าสำเร็จ");
    }

    [Authorize(Roles = "OWNER")]
    [HttpPut("users/{userId:guid}/role")]
    [ProducesResponseType(typeof(ApiResponse<StoreUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUserRole([FromRoute] Guid userId, [FromBody] UpdateUserStoreRoleRequest request, CancellationToken ct)
    {
        var updated = await _storeService.UpdateUserStoreRoleAsync(userId, request, ct);
        return Success(updated, "อัปเดตสิทธิ์ผู้ใช้เรียบร้อยแล้ว");
    }
}
