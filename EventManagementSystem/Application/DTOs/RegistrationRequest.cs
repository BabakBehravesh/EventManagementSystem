namespace EventManagementSystem.Application.DTOs;
public record RegistrationRequest
(
    string Name,
    string? PhoneNumber,
    string Email 
);
