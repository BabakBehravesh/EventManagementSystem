using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Models;

namespace EventManagementSystem.Domain.Interfaces;

public interface IEventService
{
    Task<(IEnumerable<Event>, int)> GetEventsAsync(int pageNumber, int pageSize);

    Task<ServiceResult<Event>> GetEventByIdAsync(int eventId);

    Task<ServiceResult<Event>> CreateEventAsync(Event newEvent, string createdBy);

    Task<ServiceResult<IEnumerable<Participation>>> GetEventParticipantsAsync(int eventId); 
}