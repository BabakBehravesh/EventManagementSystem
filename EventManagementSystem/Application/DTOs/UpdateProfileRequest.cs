using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Application.DTOs;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "The {0} field is required.")]
    [StringLength(50, ErrorMessage = "The {0} must be at most {1} characters long.")]
    [Display(Name = "Username")]
    public required string UserName { get; set; }

    [Phone(ErrorMessage = "The {0} field is not a valid phone number.")]
    [Display(Name = "Phone Number")]
    public required string PhoneNumber { get; set; }
}
