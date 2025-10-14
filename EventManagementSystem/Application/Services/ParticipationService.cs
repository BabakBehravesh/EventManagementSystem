namespace EventManagementSystem.Application.Services;

using EventManagementSystem.Application.DTOs;
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

    public async Task<ServiceResult<Participation>> ParticipateInEventAsync(int eventId, Participation participation)
    {
        var eventEntity = await _context.Events.FindAsync(eventId);
        if (eventEntity == null)
        { 
            return ServiceResult<Participation>.FailureResult("Event not found.");
        }

        if (eventEntity.StartTime < DateTime.Now)
        { 
            return ServiceResult<Participation>.FailureResult("Cannot register for past events.");
        }

        var alreadyRegistered = await _context.Participations.AnyAsync(r => r.EventId == eventId && r.Email == participation.Email.ToLower());
        if (alreadyRegistered)
        { 
            return ServiceResult<Participation>.FailureResult("You have been already refistered to this event.");
        }

        participation.EventId = eventId; 
        _context.Participations.Add(participation);
        await _context.SaveChangesAsync();

        await SendTicketAsEmailAsync(participation);
        return ServiceResult<Participation>.SuccessResult(participation, "Registered successfully!");
    }

    public async Task<IEnumerable<Participation>> GetParticipantsInEventAsync(int eventId)
    {
        return await _context.Participations
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