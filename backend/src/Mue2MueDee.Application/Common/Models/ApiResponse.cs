namespace Mue2MueDee.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public ApiResponse() { }

    public ApiResponse(bool success, int statusCode, string message, T? data = default, List<string>? errors = null)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
        Data = data;
        Errors = errors ?? new List<string>();
        Timestamp = DateTimeOffset.UtcNow;
    }

    public static ApiResponse<T> Ok(T data, string message = "สำเร็จ", int statusCode = 200)
    {
        return new ApiResponse<T>(true, statusCode, message, data);
    }

    public static ApiResponse<T> Fail(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new ApiResponse<T>(false, statusCode, message, default, errors);
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message = "สำเร็จ", int statusCode = 200)
    {
        return new ApiResponse
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Data = null
        };
    }

    public static new ApiResponse Fail(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors ?? new List<string>(),
            Data = null
        };
    }
}
