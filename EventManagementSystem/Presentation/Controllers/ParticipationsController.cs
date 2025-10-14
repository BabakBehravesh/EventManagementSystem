namespace EventManagementSystem.Presentation.Controllers;

using AutoMapper;
using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Application.Services;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using EventManagementSystem.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


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
    public async Task<ActionResult<ApiResponse<IEnumerable<ParticipationResponse>>>> GetParticipantsInEvent(int eventId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<IEnumerable<ParticipationResponse>>.ValidationFailure(ModelState.GetErrors()));
        }

        var participants = await _registrationService.GetParticipantsInEventAsync(eventId);

        if(participants == null || !participants.Any())
        { 
            return NotFound(ApiResponse<IEnumerable<ParticipationResponse>>.FailureResult("No participants found for the given eventId"));
        }

        var result =_mapper.Map<IEnumerable<ParticipationResponse>>(participants);

        return Ok(ApiResponse<IEnumerable<ParticipationResponse>>.SuccessResult(result));
    }

    [HttpPost]
    [Authorize(Roles = "EventParticipant")]
    public async Task<ActionResult<ApiResponse<ParticipationResponse>>> Participate(int eventId, [FromBody] ParticipationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ParticipationResponse>.ValidationFailure(ModelState.GetErrors()));
        }

        var registrationEntity = _mapper.Map<Participation>(request);

        var participationResult = await _registrationService.ParticipateInEventAsync(eventId, registrationEntity);

        if (!participationResult.Success) return BadRequest(ApiResponse.FailureResult(nameof(ParticipationService.ParticipateInEventAsync), participationResult.Errors));


        var result = _mapper.Map<ParticipationResponse>(participationResult.Data);

        return Ok(ApiResponse<ParticipationResponse>.SuccessResult(result));
    }
}