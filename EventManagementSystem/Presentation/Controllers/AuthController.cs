using EventManagementSystem.Application.DTOs.Auth;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
using EventManagementSystem.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IAuthService _authService;

    public AuthController(UserManager<ApplicationUser> userManager, 
                            RoleManager<IdentityRole> roleManager, 
                            IJwtTokenGenerator jwtTokenGenerator,
                            IAuthService authService)
    { 
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _authService = authService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserInfo>>> Register([FromBody] RegisterRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ValidationFailure(ModelState.GetErrors()));
        }

        var createdBy = await _userManager.GetUserAsync(User);
        
        if (createdBy == null)
        {
            return BadRequest(ApiResponse.FailureResult("No valid creator information for this registration."));
        }

        var authResult = await _authService.RegisterAsync(model, createdBy);

        if (authResult.Success)
        {
            return Ok(ApiResponse<UserInfo>.SuccessResult(authResult.Data, "User registered successfully!", StatusCodes.Status201Created));
        }

        return BadRequest(ApiResponse<UserInfo>.FailureResult("User registeration failed!", authResult.Errors ?? [authResult.Message]));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<UserInfo>>> ChangePassword([FromBody] ChangePasswordRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<UserInfo>.ValidationFailure(ModelState.GetErrors()));
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var result = await _authService.ChangePasswordAsync(userId, model);
        
        if (!result.Success)
        {
            return BadRequest(ApiResponse<UserInfo>.FailureResult("Password change failed!", result.Errors ?? [result.Message]));
        }
        
        return Accepted(ApiResponse<UserInfo>.SuccessResult(result.Data, "Password changes successfully!", StatusCodes.Status202Accepted));
    }

    [Authorize]
    [HttpGet("load-user-profile")]
    public async Task<ActionResult<ApiResponse<UserInfo>>> LoadUserProfile()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<UserInfo>.ValidationFailure(ModelState.GetErrors()));
        }
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (userId == null)
        {
            return BadRequest(ApiResponse.FailureResult("User ID not found."));
        }

        var result = await _authService.LoadUserProfileAsync(userId);

        if (result.Success)
        {
            return Ok(ApiResponse<UserInfo>.SuccessResult(result!.Data));
        }

        return BadRequest(ApiResponse.FailureResult(nameof(_authService.LoadUserProfileAsync), result.Errors ?? [result.Message]));
    }

    [Authorize]
    [HttpPut("change-user-profile")]
    public async Task<ActionResult<ApiResponse<UserInfo>>> ChangeUserProfile([FromBody] UserProfileRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ValidationFailure(ModelState.GetErrors()));
        }
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var result = await _authService.ChangeUserProfileAsync(userId, model);
        
        if (result.Success)
        {
            return Ok(ApiResponse<UserInfo>.SuccessResult(result.Data, "User profile changes successfully!", StatusCodes.Status202Accepted));
        }
        return BadRequest(ApiResponse.FailureResult(nameof(_authService.ChangeUserProfileAsync), result.Errors ?? [result.Message]));
    }

    [Authorize("Admin")]
    [HttpDelete]
    public async Task<ActionResult<ApiResponse<UserInfo>>> DeleteUser([FromQuery] string userId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ValidationFailure(ModelState.GetErrors()));
        }
        var result = await _authService.DeleteUserAsync(userId);
        if (result.Success)
        {
            return Ok(ApiResponse<UserInfo>.SuccessResult(result.Data));
        }
        return BadRequest(ApiResponse<UserInfo>.FailureResult(nameof(_authService.DeleteUserAsync), result.Errors ?? [result.Message]));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<UserInfo>>> Login([FromBody] LoginRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ValidationFailure(ModelState.GetErrors()));
        }

        if (User.Identity.IsAuthenticated)
        { 
            return BadRequest(ApiResponse.FailureResult("The user is already authenticated."));
        }
        
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized(ApiResponse.FailureResult("Invalid login attempt."));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        return Ok(ApiResponse<UserInfo>.AuthSuccessResult(new UserInfo { FirstName = user.FirstName, LastName = user.LastName, Email = user.Email, Roles = roles.ToList() }, token, "Login successful!"));
    }
}
