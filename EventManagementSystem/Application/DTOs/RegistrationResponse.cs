namespace EventManagementSystem.Application.DTOs
{
    public record RegistrationResponse(
        int Id,
        string Name,
        string? PhoneNumber,
        string Email,
        int EventId,
        string EventName
    );
}