using EventManagementSystem.Application.DTOs.Auth;
using EventManagementSystem.Domain.Interfaces;
using EventManagementSystem.Domain.Models;
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
            return BadRequest(ModelState);
        }

        var createdBy = await _userManager.GetUserAsync(User);
        
        if (createdBy == null)
        {
            return BadRequest();
        }

        var authResult = await _authService.RegisterAsync(model, createdBy);

        if (authResult.Success)
        {
            return Ok(new { Message = authResult.Message });
        }

        return BadRequest(authResult);
    }


    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<AuthResult>> ChangePassword([FromBody] ChangePasswordRequest model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(AuthResult.ValidationFailure(
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var result = await _authService.ChangePasswordAsync(userId, model);
        return result;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        if (User.Identity.IsAuthenticated)
        { 
            return BadRequest("The user is already authenticated.");
        }
        
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized(new { Message = "Invalid login attempt." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        return Ok(new { Token = token, Message = "Login successful!" });
    }
}
