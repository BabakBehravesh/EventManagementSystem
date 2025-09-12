namespace EventManagementSystem.Application.Services;

using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using global::EventManagementSystem.Domain.Models;
using global::EventManagementSystem.Infrastructure.Storage;
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
        //// Check if event exists
        var eventEntity = await _context.Events.FindAsync(eventId);
        if (eventEntity == null)
            return new RegistrationResult(false, Message: "Event not found.");

        //// Create new registration
        //var registration = new Registration
        //{
        //    Name = request.Name,
        //    PhoneNumber = request.PhoneNumber ?? string.Empty,
        //    Email = request.Email,
        //    EventId = eventId
        //};

        //_context.Registrations.Add(registration);
        //await _context.SaveChangesAsync();

        //var response = new RegistrationResponse(
        //    registration.Id,
        //    registration.Name,
        //    registration.PhoneNumber,
        //    registration.Email,
        //    eventId,
        //    eventEntity.Name
        //);

        //return new RegistrationResult(true, response, "Registered successfully!");
        registration.EventId = eventId; // Set the FK
        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync();
        return new RegistrationResult(true, registration, "Registered successfully!");
    }

    public async Task<IEnumerable<Registration>> GetRegistrationsForEventAsync(int eventId)
    {
        return await _context.Registrations
            .Where(r => r.EventId == eventId)
            .ToListAsync();
    }
}