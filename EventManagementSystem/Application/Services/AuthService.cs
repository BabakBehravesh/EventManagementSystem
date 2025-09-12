using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;

namespace EventManagementSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        public Task<AuthResult> LoginAsync(LoginRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            throw new NotImplementedException();
        }

        public Task SeedAdminUserAsync()
        {
            throw new NotImplementedException();
        }
    }
}
