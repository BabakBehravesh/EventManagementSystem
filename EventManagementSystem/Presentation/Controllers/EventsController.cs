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
    public async Task<IActionResult> GetEventsAsync(int pageNumber = 1, int pageSize = 10)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponseFactory.ValidationFailure(ModelState.GetErrors()); 
        }

        _requestCount++;
        var cacheKey = $"events_in_pageNumber:{pageNumber}_pageSize:{pageSize}";
        IEnumerable<Event> events;
        int totalCount = 0;

        if(!_memoryCache.TryGetValue(cacheKey, out IEnumerable<EventResponse> cachedMemory))
        {
            _cacheHitCount++;
            _logger.LogInformation($"Request: {_requestCount}, Cache Hits: {_cacheHitCount}, Hit Rate: {(_cacheHitCount * 100.0 / _requestCount)}");
            
            (events, totalCount) = await _eventService.GetEventsAsync(pageNumber, pageSize);
            cachedMemory = _mapper.Map<IEnumerable<EventResponse>>(events);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(1))
                .SetPriority(CacheItemPriority.High)
                .SetSize(1024 * 1024);

            _memoryCache.Set(cacheKey, cachedMemory, cacheOptions);
        }

        return ApiResponseFactory.PagedOk(cachedMemory, pageNumber, pageSize, totalCount, "Fetched successfully!");
    }

    [HttpGet("{eventId:int}", Name = "GetEventByIdAsync")]
    [Authorize]
    public async Task<IActionResult> GetEventByIdAsync(int eventId)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponseFactory.ValidationFailure(ModelState.GetErrors());
        }

        var serviceResult = await _eventService.GetEventByIdAsync(eventId);

        if (!serviceResult.Success)
        {
            return ApiResponseFactory.ServiceFailed(
                $"The service failed in: {_eventService.GetEventByIdAsync}.",
                serviceResult.Errors.Any() ? serviceResult.Errors.ToArray() : [serviceResult.Message]);
        }

        if (serviceResult.Data == null)
        { 
            return ApiResponseFactory.NotFound($"EventId: {eventId} is not found.");
        }

        var eventResponse = _mapper.Map<EventResponse>(serviceResult.Data);
        _logger.LogInformation($"Event information with Id: {eventId} returned successfully.");

        return ApiResponseFactory.Ok(eventResponse);
    }

    [HttpPost]
    [Authorize(Roles = "EventCreator")]
    public async Task<IActionResult> CreateEventAsync(EventRequest newEvent)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponseFactory.ValidationFailure(ModelState.GetErrors());
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponseFactory.UnAuthorized("User identity is invalid or not found.");
        }

        var eventEntity = _mapper.Map<Event>(newEvent);

        var serviceResult = await _eventService.CreateEventAsync(eventEntity, userId!);

        if (!serviceResult.Success)
        {
            return ApiResponseFactory.ServiceFailed(
                $"The service failed in: {_eventService.CreateEventAsync}.",
                serviceResult.Errors.Any() ? serviceResult.Errors.ToArray() : [serviceResult.Message]);
        }

        var eventResponse = _mapper.Map<EventResponse>(serviceResult.Data);

        return ApiResponseFactory.Created(eventResponse, $"EventId: {eventResponse.Id} created successfully!");
    }
}

