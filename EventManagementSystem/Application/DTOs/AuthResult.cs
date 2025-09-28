namespace EventManagementSystem.Application.DTOs;

using Microsoft.AspNetCore.Identity;
public class AuthResult
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? Expiration { get; set; }
    public UserInfo? UserInfo { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();

    // Default constructor for serialization
    public AuthResult() { }

    // Constructor for backward compatibility
    public AuthResult(bool success, string token = "", string message = "", IEnumerable<IdentityError>? errors = null)
    {
        Success = success;
        Token = token;
        Message = message;
        Errors = errors?.Select(e => e.Description) ?? new List<string>();
    }

    // New constructor for enhanced functionality
    public AuthResult(bool success, string message = "", string token = "",
                     DateTime? expiration = null, UserInfo? userInfo = null,
                     IEnumerable<string>? errors = null)
    {
        Success = success;
        Message = message;
        Token = token;
        Expiration = expiration;
        UserInfo = userInfo;
        Errors = errors ?? new List<string>();
    }

    // Static helper methods for common scenarios
    public static AuthResult SuccessResult(string message, string token = "", UserInfo? userInfo = null)
    {
        return new AuthResult(
            success: true,
            message: message,
            token: token,
            expiration: DateTime.UtcNow.AddHours(1),
            userInfo: userInfo
        );
    }


    public static AuthResult FailureResult(string message, IEnumerable<IdentityError>? errors = null)
    {
        return new AuthResult(
            success: false,
            message: message,
            errors: errors?.Select(e => e.Description)
        );
    }

    public static AuthResult ValidationFailure(IEnumerable<string> errors)
    {
        return new AuthResult(
            success: false,
            message: "Validation failed",
            errors: errors
        );
    }

    // Implicit conversion from the old record style for backward compatibility
    public static implicit operator AuthResult((bool Success, string Token, string Message, IEnumerable<IdentityError>? Errors) tuple)
    {
        return new AuthResult(tuple.Success, tuple.Token, tuple.Message, tuple.Errors);
    }
}
