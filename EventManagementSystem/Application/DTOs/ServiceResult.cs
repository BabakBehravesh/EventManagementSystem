using NuGet.Common;

namespace EventManagementSystem.Application.DTOs;

public class ServiceResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public IEnumerable<string> Errors { get; set; } = new List<string>();

    public int StatusCode { get; set; } = 200;

    public static ServiceResult SuccessResult(string message = "")
    {
        return new ServiceResult
        {
            Success = true,
            Message = message,
        };
    }

    public static ServiceResult FailureResult(string message, IEnumerable<string>? errors = null)
    {
        return new ServiceResult 
        { 
            Success = false, 
            Message = message, 
            Errors = errors ?? [] 
        };
    }
}

public class  ServiceResult<T>: ServiceResult
{
    public T? Data { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime? TokenExpiration { get; set; }

    public static ServiceResult<T> SuccessResult(T data, string message = "")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static new ServiceResult<T> FailureResult(
        string message = "",
        IEnumerable<string>? errors = null,
        int statusCode = 400)
    {
        var mergedErrors = new List<string>();

        // Always include the message as an error if it's non-empty
        if (!string.IsNullOrWhiteSpace(message))
            mergedErrors.Add(message);

        // Add any existing errors
        if (errors != null)
            mergedErrors.AddRange(errors.Where(e => !string.IsNullOrWhiteSpace(e)));

        // Remove duplicates and nulls
        var distinctErrors = mergedErrors
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Distinct()
            .ToList();

        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Errors = distinctErrors,
            StatusCode = statusCode
        };
    }


    public static ServiceResult<T> AuthSuccessResult(T data, string token, string message = "")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Token = token,
            TokenExpiration = DateTime.UtcNow.AddHours(1), 
        };
    }

    public static ServiceResult<T> ValidationFailure(IEnumerable<string> errors)
    {
        return new ServiceResult<T> {
            Success = false,
            Message = "Validation failed",
            Errors = errors ?? []
        };
    }
}
