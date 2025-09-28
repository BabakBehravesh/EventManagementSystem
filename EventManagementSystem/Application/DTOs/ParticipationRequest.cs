using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Application.DTOs;
public record ParticipationRequest
(
    [Required] 
    string Name,

    string? PhoneNumber,

    [Required]
    [EmailAddress]
    string Email 
);
