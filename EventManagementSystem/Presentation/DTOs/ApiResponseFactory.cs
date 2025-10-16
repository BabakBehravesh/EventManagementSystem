using Microsoft.AspNetCore.Mvc;

namespace EventManagementSystem.Presentation.DTOs;

public static class ApiResponseFactory
{
    public static IActionResult Ok<T>(T data, string message = "OK") =>
        new OkObjectResult(ApiResponse<T>.SuccessResult(data, message, StatusCodes.Status200OK));

    public static IActionResult Ok(string message = "OK") =>
        new OkObjectResult(ApiResponse.SuccessResult(message, StatusCodes.Status200OK));

    public static IActionResult Created<T>(T data, string message = "Created successfully") =>
        new CreatedResult(string.Empty, ApiResponse<T>.SuccessResult(data, message, StatusCodes.Status201Created));

    public static IActionResult BadRequest(string message, IEnumerable<string>? errors = null) =>
        new BadRequestObjectResult(ApiResponse.FailureResult(message, errors));

    public static IActionResult ValidationFailure(IEnumerable<string> errors) =>
        new BadRequestObjectResult(ApiResponse.ValidationFailure(errors));

    public static IActionResult UnAuthorized(string message = "Unauthorized") =>
        new UnauthorizedObjectResult(ApiResponse.FailureResult(message));

    public static IActionResult Forbidden(string message = "Forbidden") =>
        new ObjectResult(ApiResponse.FailureResult(message))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };

    public static IActionResult NotFound(string message) =>
        new NotFoundObjectResult(ApiResponse.FailureResult(message));

    public static IActionResult NotFound<T>(string message = "Resource not found") =>
        new NotFoundObjectResult(ApiResponse<T>.FailureResult(message));

    public static IActionResult ServiceFailed(string message = "Service failed to complete the operation", IEnumerable<string>? errors = null)
    {
        return new ObjectResult(ApiResponse.FailureResult(message, errors))
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }

    public static IActionResult ServiceFailed<T>(string message = "Service failed to complete the operation", IEnumerable<string>? errors = null)
    {
        return new ObjectResult(ApiResponse<T>.FailureResult(message, errors))
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }

    public static IActionResult Server(string message = "Internal server error", IEnumerable<string>? errors = null) =>
        new ObjectResult(ApiResponse.FailureResult(message, errors))
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

    public static IActionResult PagedOk<T>(
        IEnumerable<T> data,
        int pageNumber,
        int pageSize, 
        int totalCount,
        string message ="Request successful")
    {
        var response = ApiPaginatedResponse<T>.SuccessResult(data, pageNumber, pageSize, totalCount, message);
        return new OkObjectResult(response);
    }
}
