namespace EventManagementSystem.Application.DTOs;

public record ParticipationResponse(
    int Id,
    string Name,
    string? PhoneNumber,
    string Email,
    int EventId,
    string EventName
);