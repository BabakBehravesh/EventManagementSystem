using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Application.DTOs;
public class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    public required string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
    public required string ConfirmPassword { get; set; }

    [Required]
    public required string Token { get; set; }
}

public class ResetPasswordResponse
{
    public required bool Success { get; set; }
    public required string Message { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();
}
