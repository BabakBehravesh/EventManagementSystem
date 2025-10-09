using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Models;

namespace EventManagementSystem.Domain.Interfaces;

public interface IParticipationService
{
    Task<ServiceResult<Participation>> ParticipateInEventAsync(int eventId, Participation registration);
    Task<IEnumerable<Participation>> GetParticipantsInEventAsync(int eventId);
}

public record ParticipationResult(bool Success, Participation? Participation = null, string Message = "");