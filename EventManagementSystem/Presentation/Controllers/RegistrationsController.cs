using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementSystem.Presentation.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/[controller]")] // Route: /api/events/1/registrations
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationsController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    // GET: api/events/5/registrations
    [HttpGet]
    [Authorize(Roles = "EventCreator")] // Only creators can see all registrations for their event
    public async Task<ActionResult<IEnumerable<RegistrationResponse>>> GetRegistrationsForEvent(int eventId)
    {
        var registrations = await _registrationService.GetRegistrationsForEventAsync(eventId);
        return Ok(registrations);
    }

    // POST: api/events/5/registrations
    [HttpPost]
    [AllowAnonymous] // Or [Authorize(Roles = "EventParticipant")] if you require login to register
    public async Task<ActionResult<RegistrationResponse>> Register(int eventId, [FromBody] RegistrationRequest request)
    {
        // Map from API DTO to Domain Entity
        var registrationEntity = new Registration
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber ?? string.Empty, // Handle null
            Email = request.Email
        };

        

        var result = await _registrationService.RegisterForEventAsync(eventId, registrationEntity);

        // Map from Domain Entity back to API Response DTO
        var response = new RegistrationResponse(
            result.Registration.Id,
            result.Registration.Name,
            result.Registration.PhoneNumber,
            result.Registration.Email,
            result.Registration.EventId,
            result.Registration.Event?.Name! // From included navigation property
        );

        return Ok(response);
    }
}