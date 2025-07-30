namespace PoliMarket.Models.Common;

/// <summary>
/// Standard API response wrapper for all component operations
/// </summary>
/// <typeparam name="T">Type of data being returned</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "Operation completed successfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponse<T> ErrorResult(string message, string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = new List<string> { error }
        };
    }
}

/// <summary>
/// Non-generic API response for operations that don't return data
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }

    public static ApiResponse SuccessResult(string message = "Operation completed successfully")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>(),
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse ErrorResult(string message, string error)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = new List<string> { error },
            Timestamp = DateTime.UtcNow
        };
    }
}
