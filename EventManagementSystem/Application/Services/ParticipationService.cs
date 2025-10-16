﻿namespace EventManagementSystem.Application.Services;

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
    private readonly IEventService _eventService;

    public ParticipationService(ApplicationDbContext context, IEmailService emailService, IEventService eventService)
    {
        _context = context;
        _emailService = emailService;
        _eventService = eventService;
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

    public async Task<ServiceResult<IEnumerable<Participation>>> GetParticipantsInEventAsync(int eventId)
    {
        try
        {
            var eventObject = await _eventService.GetEventByIdAsync(eventId);

            var participants = await _context.Participations
                .Include(r => r.Event)
                .Where(r => r.EventId == eventId)
                .ToListAsync();

            if (participants.Count == 0)
            {
                return ServiceResult<IEnumerable<Participation>>.FailureResult(
                    "No participants for this event.", 
                    eventObject?.Errors);
            }

            return ServiceResult<IEnumerable<Participation>>.SuccessResult(participants, $"Participants for event: {eventId} retrieved successfully!");
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<Participation>>.FailureResult(ex.Message);
        }
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