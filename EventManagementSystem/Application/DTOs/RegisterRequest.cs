using System.ComponentModel.DataAnnotations;

namespace EventManagementSystem.Application.DTOs;

public record RegisterRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}