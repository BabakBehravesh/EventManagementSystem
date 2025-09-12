using EventManagementSystem.Domain.Models;

namespace EventManagementSystem.Domain.Interfaces;

public interface IEventService
{
    // For lists: return a summary with only essential info
    Task<IEnumerable<Event>> GetAllEventsAsync();

    // For getting a single event: return all details
    Task<Event> GetEventByIdAsync(int id);

    // For creation: return the full detail of the created resource, including its new ID
    Task<Event> CreateEventAsync(Event newEvent);

    Task<IEnumerable<Registration>> GetEventParticipantsAsync(int eventId); // Use ID, not name
}