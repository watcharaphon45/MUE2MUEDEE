using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.Common.Models;
using Mue2MueDee.Application.DTOs;
using Mue2MueDee.Application.Services;
using Mue2MueDee.Web.Controllers.Common;

namespace Mue2MueDee.Web.Controllers;

public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var response = await _authService.LoginAsync(request, ip, ct);
        return Success(response, "เข้าสู่ระบบสำเร็จ");
    }

    [Authorize]
    [HttpPost("switch-store")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SwitchStore([FromBody] SwitchStoreRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var response = await _authService.SwitchStoreAsync(request, ip, ct);
        return Success(response, "สลับร้านค้าเรียบร้อยแล้ว");
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var response = await _authService.RefreshTokenAsync(request, ip, ct);
        return Success(response, "ต่ออายุเซสชันสำเร็จ");
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _authService.RevokeTokenAsync(request.RefreshToken, ip, ct);
        return Success("ออกจากระบบเรียบร้อยแล้ว");
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var response = await _authService.RegisterStoreWithOwnerAsync(request, ip, ct);
        return CreatedSuccess(response, "ลงทะเบียนร้านค้าและผู้ดูแลสำเร็จ");
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<AuthUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var user = await _authService.GetCurrentUserProfileAsync(ct);
        if (user == null) return Unauthorized(ApiResponse.Fail("ไม่พบข้อมูลผู้ใช้", 401));
        return Success(user, "ดึงข้อมูลผู้ใช้สำเร็จ");
    }
}
