using EventManagementSystem.Domain.Models;

namespace EventManagementSystem.Domain.Interfaces;

public interface IEventService
{
    Task<IEnumerable<Event>> GetAllEventsAsync();

    Task<Event> GetEventByIdAsync(int id);

    Task<Event> CreateEventAsync(Event newEvent, string createdBy);

    Task<IEnumerable<Registration>> GetEventParticipantsAsync(int eventId); 
}