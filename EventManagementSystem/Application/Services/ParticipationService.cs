namespace EventManagementSystem.Application.Services;

using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using EventManagementSystem.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

public class ParticipationService : IParticipationService
{
    private readonly ApplicationDbContext _context;

    public ParticipationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ParticipationResult> ParticipateInEventAsync(int eventId, Participation registration)
    {
        var eventEntity = await _context.Events.FindAsync(eventId);
        if (eventEntity == null)
            return new ParticipationResult(false, Message: "Event not found.");

        var alreadyRegistered = await _context.Registrations.AnyAsync(r => r.EventId == eventId && r.Email == registration.Email.ToLower());
        if (alreadyRegistered)
            return new ParticipationResult(false, Message: "You have been already refistered to this event.");

        registration.EventId = eventId; 
        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync();
        return new ParticipationResult(true, registration, "Registered successfully!");
    }

    public async Task<IEnumerable<Participation>> GetParticipantsInEventAsync(int eventId)
    {
        return await _context.Registrations
            .Include(r => r.Event)
            .Where(r => r.EventId == eventId)
            .ToListAsync();
    }
}