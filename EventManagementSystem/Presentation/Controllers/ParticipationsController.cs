using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementSystem.Presentation.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/[controller]")] 
public class ParticipationsController : ControllerBase
{
    private readonly IParticipationService _registrationService;

    public ParticipationsController(IParticipationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpGet]
    [Authorize(Roles = "EventCreator")] 
    public async Task<ActionResult<IEnumerable<ParticipationResponse>>> GetParticipantsInEvent(int eventId)
    {
        var registrations = await _registrationService.GetParticipantsInEventAsync(eventId);
        var result = registrations.Select(r => 
                new ParticipationResponse(
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
    public async Task<ActionResult<ParticipationResponse>> Participate(int eventId, [FromBody] ParticipationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var registrationEntity = new Participation
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber ?? string.Empty, 
            Email = request.Email
        };

        var result = await _registrationService.ParticipateInEventAsync(eventId, registrationEntity);

        if (!result.Success) return BadRequest();

        var response = new ParticipationResponse(
            result.Participation!.Id,
            result.Participation.Name,
            result.Participation.PhoneNumber,
            result.Participation.Email,
            result.Participation.EventId,
            result.Participation.Event?.Name! 
        );

        return Ok(response);
    }
}