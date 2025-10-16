using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Models;

namespace EventManagementSystem.Domain.Interfaces;

public interface IParticipationService
{
    Task<ServiceResult<Participation>> ParticipateInEventAsync(int eventId, Participation registration);
    Task<ServiceResult<IEnumerable<Participation>>> GetParticipantsInEventAsync(int eventId);
}
