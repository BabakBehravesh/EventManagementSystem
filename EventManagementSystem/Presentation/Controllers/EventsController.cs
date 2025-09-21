using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Application.Types;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventManagementSystem.Controllers;

[ApiController]
[EnableCors(PolicyName = "AllowUIApp")]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    [Authorize(Roles = "EventParticipant")]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetEventsAsync()
    {
        if (!ModelState.IsValid)
        {  
            return BadRequest(ModelState); 
        }

        var events = await _eventService.GetAllEventsAsync();
        var result = events.Select(e => new EventResponse(
            e.Id,
            e.Name,
            e.Description,
            e.Location,
            e.StartTime,
            e.EndTime));

         return Ok(result);
    }

    [HttpGet("{eventId}")]
    [Authorize(Roles = "EventParticipant")]
    public async Task<ActionResult<EventResponse>> GetEventByIdAsync(int eventId)
    {

        var eventObject = await _eventService.GetEventByIdAsync(eventId);
        if (eventObject == null)
            return NotFound();

        var eventResponse = new EventResponse
        (
            eventObject.Id,
            eventObject.Name,
            eventObject.Description,
            eventObject.Location,
            eventObject.StartTime,
            eventObject.EndTime
        );
        return Ok(eventResponse);
    }

    [HttpGet("participants/{eventId}")]
    [Authorize(Roles = "EventCreator")]
    public async Task<ActionResult<IEnumerable<RegistrationResponse>>> GetEventParticipantsByEventId(int eventId)
    {
        try
        {
            var participants = await _eventService.GetEventParticipantsAsync(eventId);
            var result = participants.Select(p => new RegistrationResponse(
                p.Id,
                p.Name,
                p.PhoneNumber,
                p.Email,
                p.EventId,
                EventName: p.Event!.Name));

            return Ok(participants);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Roles = "EventCreator")]
    public async Task<ActionResult<EventResponse>> CreateEventAsync(EventRequest newEvent)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User identity is invalid or not found.");
        }

        try
        {
            var eventEntity = new Event
            {
                Name = newEvent.Name,
                Description = newEvent.Description,
                Location = newEvent.Location,
                StartTime = newEvent.StartTime,
                EndTime = newEvent.EndTime,
                CreatedBy = userId 
            };

            var createdEvent = await _eventService.CreateEventAsync(eventEntity);

            var eventResponse = new EventResponse(
                createdEvent.Id,
                createdEvent.Name,
                createdEvent.Description,
                createdEvent.Location,
                createdEvent.StartTime,
                createdEvent.EndTime
            );

            return Created();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}