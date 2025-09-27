using AutoMapper;
using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventManagementSystem.Controllers;

[ApiController]
[EnableCors]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IMapper _mapper;

    public EventsController(IEventService eventService, IMapper mapper)
    {
        _eventService = eventService;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetEventsAsync()
    {
        if (!ModelState.IsValid)
        {  
            return BadRequest(ModelState); 
        }

        var events = await _eventService.GetAllEventsAsync();
        var result = _mapper.Map<IEnumerable<EventResponse>>(events);

        return Ok(result);
    }

    [HttpGet("{eventId:int}", Name = "GetEventById")]
    [Authorize]
    public async Task<ActionResult<EventResponse>> GetEventByIdAsync(int eventId)
    {

        var eventObject = await _eventService.GetEventByIdAsync(eventId);
        if (eventObject == null)
            return NotFound();

        var eventResponse = _mapper.Map<EventResponse>(eventObject);
        return Ok(eventResponse);
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
            var eventEntity = _mapper.Map<Event>(newEvent);

            var createdEvent = await _eventService.CreateEventAsync(eventEntity, userId!);

            var eventResponse = _mapper.Map<EventResponse>(createdEvent);

            return CreatedAtAction(
                null,
                eventResponse
            );
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}