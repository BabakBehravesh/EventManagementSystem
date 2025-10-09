using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using EventManagementSystem.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;


namespace EventManagementSystem.Application.Services;

public class EventService : IEventService
{
    private readonly ApplicationDbContext _context;

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

    public async Task<ServiceResult<Event>> GetEventByIdAsync(int eventId)
    {
        var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
        {
            return ServiceResult<Event>.FailureResult($"Event with ID {eventId} was not found.");
        }

        return ServiceResult<Event>.SuccessResult(eventEntity);
    }

    public async Task<ServiceResult<IEnumerable<Participation>>> GetEventParticipantsAsync(int eventId)
    {
        var eventObject = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventObject == null)
        {
            return ServiceResult<IEnumerable<Participation>>.FailureResult($"Event with Id '{eventId}' not found");
        }

        return ServiceResult<IEnumerable<Participation>>.SuccessResult(
            await _context.Participations
                .Where(r => r.EventId == eventObject.Id)
                .AsNoTracking()
                .ToListAsync());
    }

    public async Task<ServiceResult<Event>> CreateEventAsync(Event newEvent, string createdBy)
    {
        var existingEvent = await _context.Events
            .FirstOrDefaultAsync(e =>
                e.Name == newEvent.Name &&
                e.StartTime == newEvent.StartTime &&
                e.Location == newEvent.Location);

        if (existingEvent != null)
        {
            ServiceResult<Event>.FailureResult($"An event with the same name, time, and location already exists.");
        }

        newEvent.CreatedBy = createdBy;

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return ServiceResult<Event>.SuccessResult(newEvent);
    }
}