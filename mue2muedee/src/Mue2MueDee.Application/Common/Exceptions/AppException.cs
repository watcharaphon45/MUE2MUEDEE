namespace Mue2MueDee.Application.Common.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public List<string> Errors { get; }

    public AppException(string message, int statusCode = 400, List<string>? errors = null) 
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors ?? new List<string>();
    }

    public static AppException BadRequest(string message, List<string>? errors = null) => 
        new(message, 400, errors);

    public static AppException Unauthorized(string message = "ไม่มีสิทธิ์เข้าถึงข้อมูลนี้") => 
        new(message, 401);

    public static AppException Forbidden(string message = "สิทธิ์การใช้งานของคุณไม่เพียงพอ") => 
        new(message, 403);

    public static AppException NotFound(string message = "ไม่พบข้อมูลที่ต้องการ") => 
        new(message, 404);

    public static AppException Conflict(string message) => 
        new(message, 409);
}
