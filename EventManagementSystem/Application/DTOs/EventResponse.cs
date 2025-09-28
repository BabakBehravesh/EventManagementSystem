namespace EventManagementSystem.Application.DTOs;
public record EventResponse(
    int Id, 
    string Name, 
    string Description, 
    string Location,
    DateTime StartTime, 
    DateTime EndTime
);
