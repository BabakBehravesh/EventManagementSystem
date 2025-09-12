using EventManagementSystem.Domain.Models;

namespace EventManagementSystem.Domain.Interfaces;

public interface IRegistrationService
{
    Task<RegistrationResult> RegisterForEventAsync(int eventId, Registration registration);
    Task<IEnumerable<Registration>> GetRegistrationsForEventAsync(int eventId);
}

public record RegistrationResult(bool Success, Registration Registration = null, string Message = "");