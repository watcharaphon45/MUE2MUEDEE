using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Mue2MueDee.Application.Common.Models;

namespace Mue2MueDee.Web.Configuration;

public static class ApiResponseDefaults
{
    public static void ConfigureModelValidation(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage).Where(message => !string.IsNullOrWhiteSpace(message)).ToList();
            return new BadRequestObjectResult(ApiResponse.Fail(
                errors.FirstOrDefault() ?? "ข้อมูลที่ส่งมาไม่ถูกต้อง", 400, errors));
        };
    }

    public static void ConfigureAuthenticationResponses(JwtBearerOptions options)
    {
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Headers.WWWAuthenticate = "Bearer";
                return context.Response.WriteAsJsonAsync(ApiResponse.Fail("กรุณาเข้าสู่ระบบหรือเข้าสู่ระบบใหม่เมื่อเซสชันหมดอายุ", 401));
            },
            OnForbidden = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return context.Response.WriteAsJsonAsync(ApiResponse.Fail("คุณไม่มีสิทธิ์ดำเนินการนี้", 403));
            }
        };
    }
}
