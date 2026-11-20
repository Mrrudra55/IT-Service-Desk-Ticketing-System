using System.Security.Claims;
using ITServiceDesk.DTOs.Common;
using ITServiceDesk.DTOs.Users;
using ITServiceDesk.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServiceDesk.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Retrieves a filtered, paginated list of all system users (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<UserDto>>> GetUsers([FromQuery] UserFilterDto filter)
    {
        var result = await _userService.GetUsersAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all active administrators eligible for ticket assignment.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("agents")]
    [HttpGet("admins")]
    public async Task<ActionResult<List<UserDto>>> GetAssignableAgents()
    {
        var admins = await _userService.GetAssignableAgentsAsync();
        return Ok(admins);
    }

    /// <summary>
    /// Retrieves user profile by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        var (currentUserId, currentRole) = GetCurrentUser();

        // Non-admin users can only view their own record
        if (currentRole != "Admin" && currentUserId != id)
        {
            return Forbid();
        }

        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }

    /// <summary>
    /// Creates a new user with a specified role (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto)
    {
        var created = await _userService.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetUserById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates user details (Name, Email).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var updated = await _userService.UpdateUserAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>
    /// Updates a user's role: Employee or Admin (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/role")]
    public async Task<ActionResult<UserDto>> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
    {
        var (currentAdminId, _) = GetCurrentUser();
        var updated = await _userService.UpdateUserRoleAsync(id, dto, currentAdminId);
        return Ok(updated);
    }

    /// <summary>
    /// Activates or deactivates a user account (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/active")]
    public async Task<ActionResult<UserDto>> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
    {
        var (currentAdminId, _) = GetCurrentUser();
        var updated = await _userService.UpdateUserStatusAsync(id, dto, currentAdminId);
        return Ok(updated);
    }

    private (int UserId, string UserRole) GetCurrentUser()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(idClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid authentication token.");
        }

        return (userId, roleClaim ?? "Employee");
    }
}
