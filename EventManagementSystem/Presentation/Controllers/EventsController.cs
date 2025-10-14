using AutoMapper;
using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using EventManagementSystem.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace EventManagementSystem.Controllers;

[ApiController]
[EnableCors]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<EventsController> _logger;
    private static int _requestCount = 0;
    private static int _cacheHitCount = 0;

    public EventsController(IEventService eventService, IMapper mapper, IMemoryCache memoryCache, ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _mapper = mapper;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    [HttpGet]
    [ResponseCache(Location = ResponseCacheLocation.Any , Duration = 60)]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<EventResponse>>>> GetEventsAsync()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ValidationFailure(ModelState.GetErrors())); 
        }

        _requestCount++;
        var cacheKey = "all_events_key";

        if(!_memoryCache.TryGetValue(cacheKey, out IEnumerable<EventResponse> cachedMemory))
        {
            _cacheHitCount++;
            _logger.LogInformation($"Request: {_requestCount}, Cache Hits: {_cacheHitCount}, Hit Rate: {(_cacheHitCount * 100.0 / _requestCount)}");
            
            var events = await _eventService.GetAllEventsAsync();
            cachedMemory = _mapper.Map<IEnumerable<EventResponse>>(events);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(1))
                .SetPriority(CacheItemPriority.High)
                .SetSize(1024 * 1024);

            _memoryCache.Set(cacheKey, cachedMemory, cacheOptions);
        }

        return Ok(ApiResponse<IEnumerable<EventResponse>>.SuccessResult(cachedMemory));
    }

    [HttpGet("{eventId:int}", Name = "GetEventByIdAsync")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<EventResponse>>> GetEventByIdAsync(int eventId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ValidationFailure(ModelState.GetErrors()));
        }

        var eventObject = await _eventService.GetEventByIdAsync(eventId);
        if (eventObject == null)
            return NotFound(ApiResponse<EventResponse>.FailureResult("EventId not found"));

        var eventResponse = _mapper.Map<EventResponse>(eventObject.Data);
        return Ok(ApiResponse<EventResponse>.SuccessResult(eventResponse));
    }

    [HttpPost]
    [Authorize(Roles = "EventCreator")]
    public async Task<ActionResult<ApiResponse<EventResponse>>> CreateEventAsync(EventRequest newEvent)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ValidationFailure(ModelState.GetErrors()));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse.FailureResult("User identity is invalid or not found."));
        }

        try
        {
            var eventEntity = _mapper.Map<Event>(newEvent);

            var createdEvent = await _eventService.CreateEventAsync(eventEntity, userId!);

            var eventResponse = _mapper.Map<EventResponse>(createdEvent.Data);

            return CreatedAtAction(
                null,
                ApiResponse<EventResponse>.SuccessResult(
                    eventResponse,
                    "Event created successfully")
            );
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.FailureResult(ex.Message));
        }
    }
}