using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    // The controller now depends on the service interface, not the DbContext.
    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetEventsAsync()
    {
        var events = await _eventService.GetAllEventsAsync();
        return Ok(events);
    }

    [HttpGet]
    public async Task<ActionResult<EventResponse>> GetEventByIdAsync(int eventId)
    {
        var eventObject = await _eventService.GetEventByIdAsync(eventId);
        return Ok(eventObject);
    }

    [HttpGet("participants/{eventName}")]
    [Authorize(Roles = "EventCreator")]
    public async Task<ActionResult<IEnumerable<RegistrationResponse>>> GetEventParticipantsByEventId(int eventId)
    {
        try
        {
            var participants = await _eventService.GetEventParticipantsAsync(eventId);
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
        // Get the user's ID from the JWT token (HTTP concern)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User identity is invalid or not found.");
        }

        try
        {
            // Mapping: Create an Event entity from the EventRequest DTO
            var eventEntity = new Event
            {
                Name = newEvent.Name,
                Description = newEvent.Description,
                Location = newEvent.Location,
                StartTime = newEvent.StartTime,
                EndTime = newEvent.EndTime,
                CreatedBy = userId // Set the creator from the token, NOT from the request
            };

            var createdEvent = await _eventService.CreateEventAsync(eventEntity);

            // Map the created Event entity back to an EventResponse DTO
            var eventResponse = new EventResponse(
                createdEvent.Id,
                createdEvent.Name,
                createdEvent.Description,
                createdEvent.Location,
                createdEvent.StartTime,
                createdEvent.EndTime
            );

            // Assuming you have a GetEventById endpoint
            return CreatedAtAction(nameof(GetEventByIdAsync), new { id = createdEvent.Id }, eventResponse);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}