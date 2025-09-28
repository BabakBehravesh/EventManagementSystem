using EventManagementSystem.Application.Types;
using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Application.DTOs;

public class RegisterRequest
{
    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }


    [Required(ErrorMessage = "The {0} field is required.")]
    [EmailAddress(ErrorMessage = "The {0} field is not a valid email address.")]
    [Display(Name = "Email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "The {0} field is required.")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "The {0} field is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The {0} and {1} do not match.")]
    public required string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "At least one role is required.")]
    [MinLength(1, ErrorMessage = "At least one role must be selected.")]
    public required List<RoleType> UserRoles { get; set; } = new List<RoleType>();
}