using System.Security.Claims;
using ITServiceDesk.DTOs.Auth;
using ITServiceDesk.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServiceDesk.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a standard Employee account and returns a JWT access token.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        var response = await _authService.RegisterAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Registers an Administrator account using a valid security key and returns a JWT access token.
    /// </summary>
    [HttpPost("register-admin")]
    public async Task<ActionResult<AuthResponseDto>> RegisterAdmin([FromBody] AdminRegisterRequestDto request)
    {
        var response = await _authService.RegisterAdminAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Authenticates an Employee or Administrator with email and password and returns a JWT access token.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Returns the currently authenticated user's profile based on the JWT Bearer token.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid user token claim." });
        }

        var profile = await _authService.GetCurrentUserProfileAsync(userId);
        return Ok(profile);
    }
}
