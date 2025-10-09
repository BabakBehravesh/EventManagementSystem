using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Models;

namespace EventManagementSystem.Domain.Interfaces;

public interface IEventService
{
    Task<IEnumerable<Event>> GetAllEventsAsync();

    Task<ServiceResult<Event>> GetEventByIdAsync(int eventId);

    Task<ServiceResult<Event>> CreateEventAsync(Event newEvent, string createdBy);

    Task<ServiceResult<IEnumerable<Participation>>> GetEventParticipantsAsync(int eventId); 
}