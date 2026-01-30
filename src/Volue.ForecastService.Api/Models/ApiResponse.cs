namespace Volue.ForecastService.Api.Models;

/// <summary>
/// Standard API response wrapper for consistent response format.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> SuccessResult(T data) => new()
    {
        Success = true,
        Data = data,
        Error = null
    };

    public static ApiResponse<T> FailureResult(string code, string message) => new()
    {
        Success = false,
        Data = default,
        Error = new ApiError { Code = code, Message = message }
    };
}

/// <summary>
/// API error details.
/// </summary>
public class ApiError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
