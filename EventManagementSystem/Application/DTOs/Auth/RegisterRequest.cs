using EventManagementSystem.Application.Types;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Application.DTOs.Auth;

public record RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required RoleType UserRoles { get; set; } = RoleType.EventParticipant;
}