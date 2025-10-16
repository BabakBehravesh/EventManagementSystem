﻿using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using EventManagementSystem.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;


namespace EventManagementSystem.Application.Services;

public class EventService : IEventService
{
    private readonly ApplicationDbContext _context;

    public EventService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Event>, int)> GetEventsAsync(int pageNumber, int pageSize)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = _context.Events.AsQueryable();

        var totalCount = await query.CountAsync();

        var events = await query
            .AsNoTracking()
            .Skip((pageNumber -1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (events, totalCount);
    }

    public async Task<ServiceResult<Event>> GetEventByIdAsync(int eventId)
    {
        var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
        {
            return ServiceResult<Event>.FailureResult($"Event with ID {eventId} was not found.", [$"Event with ID {eventId} was not found."]);
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
            return ServiceResult<Event>.FailureResult("An event with the same name, start time and location already exists.", statusCode: 400);
        }

        newEvent.CreatedBy = createdBy;

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return ServiceResult<Event>.SuccessResult(newEvent, "Event created successfully!");
    }
}