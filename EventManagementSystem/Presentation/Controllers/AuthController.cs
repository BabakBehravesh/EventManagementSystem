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
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponseFactory.ValidationFailure(ModelState.GetErrors());
        }

        var createdBy = await _userManager.GetUserAsync(User);
        
        if (createdBy == null)
        {
            return ApiResponseFactory.NotFound($"No valid creator information for this registration: {model.Email}");
        }

        var authResult = await _authService.RegisterAsync(model, createdBy);

        if (!authResult.Success)
        {
            return ApiResponseFactory.ServiceFailed(
                $"User registeration failed in service {_authService.RegisterAsync}.", 
                authResult.Errors.Any() ? authResult.Errors.ToArray() : [authResult.Message]);
        }

        return ApiResponseFactory.Created(authResult.Data, "User registered successfully!");
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest model)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponseFactory.ValidationFailure(ModelState.GetErrors());
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var result = await _authService.ChangePasswordAsync(userId, model);
        
        if (!result.Success)
        {
            return ApiResponseFactory.ServiceFailed(
                $"The service failed to change Password in: {nameof(_authService.ChangePasswordAsync)}.",
                result.Errors.Any() ? result.Errors.ToArray() : [result.Message]);
        }
        
        return ApiResponseFactory.Ok(result.Data, "Password changes successfully!");
    }

    [Authorize]
    [HttpGet("load-user-profile")]
    public async Task<IActionResult> LoadUserProfile()
    {
        if (!ModelState.IsValid)
        {
            return ApiResponseFactory.ValidationFailure(ModelState.GetErrors());
        }
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (userId == null)
        {
            return ApiResponseFactory.NotFound("User ID not found.");
        }

        var result = await _authService.LoadUserProfileAsync(userId);

        if (!result.Success)
        {
            return ApiResponseFactory.ServiceFailed(
                $"Service Failed: {nameof(_authService.LoadUserProfileAsync)}", 
                result.Errors.Any() ? result.Errors.ToArray() : [result.Message]
            );
        }

        return ApiResponseFactory.Ok(result!.Data);
    }

    [Authorize]
    [HttpPut("change-user-profile")]
    public async Task<IActionResult> ChangeUserProfile([FromBody] UserProfileRequest model)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponseFactory.ValidationFailure(ModelState.GetErrors());
        }
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var result = await _authService.ChangeUserProfileAsync(userId, model);
        if (!result.Success)
        {
            return ApiResponseFactory.ServiceFailed(
                $"The sevice failed in: {nameof(_authService.ChangeUserProfileAsync)}", 
                result.Errors.Any() ? result.Errors.ToArray() : [result.Message]);
        }

        return ApiResponseFactory.Ok(result.Data, $"User profile with name: {string.Join(" ", model.FirstName, model.LastName)} changes successfully!");

    }

    [Authorize("Admin")]
    [HttpDelete]
    public async Task<IActionResult> DeleteUser([FromQuery] string userId)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponseFactory.ValidationFailure(ModelState.GetErrors());
        }
        
        var result = await _authService.DeleteUserAsync(userId);
        if (!result.Success)
        {
            return ApiResponseFactory.ServiceFailed(
                $"The service failedin: {nameof(_authService.DeleteUserAsync)}", 
                result.Errors.Any() ? result.Errors.ToArray() : [result.Message]);
        }

        return ApiResponseFactory.Ok(result.Data);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        if (!ModelState.IsValid)
        {
            return ApiResponseFactory.ValidationFailure(ModelState.GetErrors());
        }

        if (User.Identity.IsAuthenticated)
        { 
            return ApiResponseFactory.BadRequest("The authenticated user cannot be authenticated again!");
        }
        
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return ApiResponseFactory.UnAuthorized("Invalid login attempt.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        return Ok(ApiResponse<UserInfo>.AuthSuccessResult(new UserInfo { FirstName = user.FirstName, LastName = user.LastName, Email = user.Email, Roles = roles.ToList() }, token, "Login successful!"));
    }
}
