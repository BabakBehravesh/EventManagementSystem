namespace EventManagementSystem.Application.DTOs.Auth
{
    public class UserProfileRequest
    {
        public required string FirstName { get; set; }

        public required string LastName { get; set; }
    }
}
