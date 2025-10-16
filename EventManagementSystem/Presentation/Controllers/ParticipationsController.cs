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
    public async Task<IActionResult> GetParticipantsInEvent(int eventId)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponseFactory.ValidationFailure(ModelState.GetErrors());
        }

        var participants = await _registrationService.GetParticipantsInEventAsync(eventId);

        var participantsData = participants.Data;

        if(participantsData == null || !participantsData.Any())
        { 
            return ApiResponseFactory.NotFound("No participants found for the given eventId");
        }

        var result =_mapper.Map<IEnumerable<ParticipationResponse>>(participantsData);

        return ApiResponseFactory.Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "EventParticipant")]
    public async Task<IActionResult> Participate(int eventId, [FromBody] ParticipationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponseFactory.ValidationFailure(ModelState.GetErrors());
        }

        var registrationEntity = _mapper.Map<Participation>(request);

        var participationResult = await _registrationService.ParticipateInEventAsync(eventId, registrationEntity);

        if (!participationResult.Success)
        { 
            return ApiResponseFactory.ServiceFailed(
                $"The sevice failed in: {nameof(ParticipationService.ParticipateInEventAsync)}", 
                participationResult.Errors.Any() ? participationResult.Errors.ToArray() : [participationResult.Message]);
        }

        var result = _mapper.Map<ParticipationResponse>(participationResult.Data);

        return ApiResponseFactory.Ok(result);
    }
}