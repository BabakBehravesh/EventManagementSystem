namespace EventManagementSystem.Application.DTOs;
public record EventResponse(int id, string name, string description, string location,
                            DateTime startTime, DateTime endTime);
