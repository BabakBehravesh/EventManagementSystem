namespace EventManagementSystem.Application.DTOs;
public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Token { get; set; }
    public DateTime? Expiration { get; set; }
    public UserInfo UserInfo { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();
}
