namespace EventManagementSystem.Application.Services;

using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using EventManagementSystem.Infrastructure.QrCode;
using EventManagementSystem.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

public class ParticipationService : IParticipationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public ParticipationService(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
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

        await SendTicketAsEmailAsync(registration);
        return new ParticipationResult(true, registration, "Registered successfully!");
    }

    public async Task<IEnumerable<Participation>> GetParticipantsInEventAsync(int eventId)
    {
        return await _context.Registrations
            .Include(r => r.Event)
            .Where(r => r.EventId == eventId)
            .ToListAsync();
    }

    private async Task SendTicketAsEmailAsync(Participation participation)
    {
        var qrData = new QRDataBuilder()
            .WithParticipant(participation.Id.ToString())
            .WithEvent(participation.EventId.ToString())
            .WithParticipantName(participation.Name)
            .WithTimestamp(DateTime.Now)
            .AsJsonFormat();

        await _emailService.SendCustomEmailAsync(
            participation.Email,
            participation.Name,
            "Entrance ticket",
            $"<p>Dear {participation.Name}, Thanks you for participating in {participation.Event!.Name} event.</p>" +
            "You can scan the below QR code at the time of entrance.",
            qrData,
            "QR Code");
    }
}