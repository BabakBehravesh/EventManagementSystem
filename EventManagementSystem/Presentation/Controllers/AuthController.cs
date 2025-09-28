using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger,
        IConfiguration configuration)
    {
        _authService = authService;
        _logger = logger;
        _configuration = configuration;
    }

    [Authorize(Roles = "EventCreator,Admin")]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(AuthResult.ValidationFailure(
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
        }

        var result = await _authService.RegisterAsync(model);
        return ResultToActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(AuthResult.ValidationFailure(
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
        }

        var result = await _authService.LoginAsync(model);
        return ResultToActionResult(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<AuthResponse>> ChangePassword([FromBody] ChangePasswordRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(AuthResult.ValidationFailure(
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var result = await _authService.ChangePasswordAsync(userId, model);
        return ResultToActionResult(result);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserInfo>> GetProfile()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var result = await _authService.GetUserProfileAsync(userId);

        if (result.Success)
        {
            return Ok(result.UserInfo);
        } 

        return BadRequest();
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<AuthResponse>> UpdateProfile([FromBody] UpdateProfileRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(AuthResult.ValidationFailure(
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var result = await _authService.UpdateProfileAsync(userId, model);
        return ResultToActionResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("assign-roles/{userId}")]
    public async Task<ActionResult<AuthResponse>> AssignRoles(string userId, [FromBody] List<string> roles)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var result = await _authService.AssignRolesAsync(userId, roles, currentUserId);
        return ResultToActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<AuthResponse>> ForgotPassword([FromBody] ForgotPasswordRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(AuthResult.ValidationFailure(
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
        }

        var result = await _authService.ForgotPasswordAsync(model.Email);
        return ResultToActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult<AuthResponse>> ResetPassword([FromBody] ResetPasswordRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(AuthResult.ValidationFailure(
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
        }

        var result = await _authService.ResetPasswordAsync(model);
        return ResultToActionResult(result);
    }

    [AllowAnonymous]
    [HttpGet("validate-reset-token")]
    public async Task<ActionResult<AuthResponse>> ValidateResetToken([FromQuery] string email, [FromQuery] string token)
    {
        var result = await _authService.ValidateResetTokenAsync(email, token);
        return ResultToActionResult(result);
    }

    // Helper method to convert AuthResult to ActionResult
    private ActionResult<AuthResponse> ResultToActionResult(AuthResult result)
    {
        var response = new AuthResponse
        {
            Success = result.Success,
            Message = result.Message,
            Token = result.Token,
            Expiration = result.Expiration,
            UserInfo = result.UserInfo,
            Errors = result.Errors
        };

        if (result.Success)
        {
            return Ok(response);
        }

        if (result.Message?.Contains("not found") == true)
        {
            return NotFound(response);
        }

        if (result.Message?.Contains("Invalid login") == true || result.Message?.Contains("Unauthorized") == true)
        {
            return Unauthorized(response);
        }

        return BadRequest(response);
    }
}