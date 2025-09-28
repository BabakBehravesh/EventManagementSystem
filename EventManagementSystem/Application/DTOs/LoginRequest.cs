using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Application.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "The {0} field is required.")]
    [EmailAddress(ErrorMessage = "The {0} field is not a valid email address.")]
    [Display(Name = "Email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "The {0} field is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public required string Password { get; set; }

    [Display(Name = "Remember Me")]
    public required bool RememberMe { get; set; }
}