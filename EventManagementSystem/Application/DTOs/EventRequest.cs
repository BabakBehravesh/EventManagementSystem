namespace EventManagementSystem.Application.DTOs;

    public record EventRequest
    (
        string Name,
        string Description,
        string Location,
        DateTime StartTime,
        DateTime EndTime
    );


