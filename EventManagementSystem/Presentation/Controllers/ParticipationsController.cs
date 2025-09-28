using AutoMapper;
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
    private readonly IMapper _mapper;

    public ParticipationsController(IParticipationService registrationService, IMapper mapper)
    {
        _registrationService = registrationService;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize(Roles = "EventCreator")] 
    public async Task<ActionResult<IEnumerable<ParticipationResponse>>> GetParticipantsInEvent(int eventId)
    {
        var participants = await _registrationService.GetParticipantsInEventAsync(eventId);

        var result =_mapper.Map<IEnumerable<ParticipationResponse>>(participants);

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

        var registrationEntity = _mapper.Map<Participation>(request);

        var participationResult = await _registrationService.ParticipateInEventAsync(eventId, registrationEntity);

        if (!participationResult.Success) return BadRequest();


        var result = _mapper.Map<ParticipationResponse>(participationResult.Participation);

        return Ok(result);
    }
}