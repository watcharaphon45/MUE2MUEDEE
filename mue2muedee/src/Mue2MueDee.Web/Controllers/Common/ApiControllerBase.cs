using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.Common.Models;

namespace Mue2MueDee.Web.Controllers.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Success<T>(T data, string message = "สำเร็จ")
    {
        return Ok(ApiResponse<T>.Ok(data, message));
    }

    protected IActionResult Success(string message = "สำเร็จ")
    {
        return Ok(ApiResponse.Ok(message));
    }

    protected IActionResult CreatedSuccess<T>(T data, string message = "สร้างข้อมูลสำเร็จ")
    {
        return StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Ok(data, message, StatusCodes.Status201Created));
    }

    protected IActionResult Fail(string message, int statusCode = StatusCodes.Status400BadRequest, List<string>? errors = null)
    {
        return StatusCode(statusCode, ApiResponse.Fail(message, statusCode, errors));
    }
}
