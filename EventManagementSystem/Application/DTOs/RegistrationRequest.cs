using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Application.DTOs;
public record RegistrationRequest
(
    [Required] 
    string Name,

    string? PhoneNumber,

    [Required]
    [EmailAddress]
    string Email 
);
