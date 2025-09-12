using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using EventManagementSystem.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;


namespace EventManagementSystem.Application.Services;

public class EventService : IEventService
{
    private readonly ApplicationDbContext _context;

    // DbContext is injected here. The service depends on the abstraction (DbContext), not the implementation.
    public EventService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync()
    {
        return await _context.Events
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Event> GetEventByIdAsync(int eventId)
    {
        var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
        {
            throw new Exception($"Event with ID {eventId} was not found.");
        }

        return eventEntity;
    }

    public async Task<IEnumerable<Registration>> GetEventParticipantsAsync(int eventId)
    {
        var eventObject = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventObject == null)
        {
            // Throw exceptions for flow control in the service layer, let the controller handle HTTP codes.
            throw new KeyNotFoundException($"Event with Id '{eventId}' not found");
        }

        return await _context.Registrations
            .Where(r => r.EventId == eventObject.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Event> CreateEventAsync(Event newEvent)
    {
        var existingEvent = await _context.Events
            .FirstOrDefaultAsync(e =>
                e.Name == newEvent.Name &&
                e.StartTime == newEvent.StartTime &&
                e.Location == newEvent.Location);

        if (existingEvent != null)
        {
            throw new Exception($"An event with the same name, time, and location already exists.");
        }

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
        return newEvent;
    }
}