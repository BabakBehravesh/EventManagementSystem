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
    [DataType(DataType.Password)]
    public required string Password { get; set; }

    [Required]
    public required Collection<RoleType> UserRoles { get; set; } = [];
}