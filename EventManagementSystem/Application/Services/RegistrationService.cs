namespace EventManagementSystem.Application.Services;

using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using EventManagementSystem.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

public class RegistrationService : IRegistrationService
{
    private readonly ApplicationDbContext _context;

    public RegistrationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RegistrationResult> RegisterForEventAsync(int eventId, Registration registration)
    {
        var eventEntity = await _context.Events.FindAsync(eventId);
        if (eventEntity == null)
            return new RegistrationResult(false, Message: "Event not found.");

        var alreadyRegistered = await _context.Registrations.AnyAsync(r => r.EventId == eventId && r.Email == registration.Email.ToLower());
        if (alreadyRegistered)
            return new RegistrationResult(false, Message: "You have been already refistered to this event.");

        registration.EventId = eventId; 
        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync();
        return new RegistrationResult(true, registration, "Registered successfully!");
    }

    public async Task<IEnumerable<Registration>> GetRegistrationsForEventAsync(int eventId)
    {
        return await _context.Registrations
            .Include(r => r.Event)
            .Where(r => r.EventId == eventId)
            .ToListAsync();
    }
}