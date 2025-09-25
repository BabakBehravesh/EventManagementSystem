using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementSystem.Presentation.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/[controller]")] 
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationsController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpGet]
    [Authorize(Roles = "EventCreator")] 
    public async Task<ActionResult<IEnumerable<RegistrationResponse>>> GetRegistrationsForEvent(int eventId)
    {
        var registrations = await _registrationService.GetRegistrationsForEventAsync(eventId);
        var result = registrations.Select(r => 
                new RegistrationResponse(
                    r.Id,
                    r.Name,
                    r.PhoneNumber,
                    r.Email,
                    r.EventId,
                    EventName: r.Event!.Name));

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "EventParticipant")]
    public async Task<ActionResult<RegistrationResponse>> Register(int eventId, [FromBody] RegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var registrationEntity = new Registration
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber ?? string.Empty, 
            Email = request.Email
        };

        var result = await _registrationService.RegisterForEventAsync(eventId, registrationEntity);

        if (!result.Success) return BadRequest();

        var response = new RegistrationResponse(
            result.Registration!.Id,
            result.Registration.Name,
            result.Registration.PhoneNumber,
            result.Registration.Email,
            result.Registration.EventId,
            result.Registration.Event?.Name! 
        );

        return Ok(response);
    }
}