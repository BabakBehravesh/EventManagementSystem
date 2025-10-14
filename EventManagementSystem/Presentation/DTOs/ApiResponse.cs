namespace EventManagementSystem.Presentation.DTOs;

public record ApiResponse
{
    public bool Success { get; set; }
    
    public string Message { get; set; } = string.Empty;

    public IEnumerable<string> Errors { get; init; } = [];

    public int StatusCode { get; init; } = 200;

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse SuccessResult(string message, int statusCode = 200) =>
        new() { Success = true, Message = message, StatusCode = statusCode };

    public static ApiResponse FailureResult(string message, IEnumerable<string>? error = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = error ?? []
        };
    }

    public static ApiResponse ValidationFailure(IEnumerable<string> errors) =>
        new() { Success = false, Message = "Validation Failed", Errors = errors, StatusCode = 400 };
}

public record ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public string? Token { get; init; }

    public DateTime? TokenExpiration { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "Request successful", int statusCode = 200) =>
        new() { Success = true, Message = message, Data = data, StatusCode = statusCode };

    public static new ApiResponse<T> FailureResult(string message, IEnumerable<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? [] };
   
    public static ApiResponse<T> AuthSuccessResult(T data, string token, string message = "") =>
        new() 
        { 
            Success = true, 
            Message = message, 
            Data = data, 
            Token = token,
            TokenExpiration = DateTime.UtcNow.AddHours(1)
        };
}
