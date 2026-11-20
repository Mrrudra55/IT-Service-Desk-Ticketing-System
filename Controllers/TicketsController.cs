using System.Security.Claims;
using ITServiceDesk.DTOs.Common;
using ITServiceDesk.DTOs.Tickets;
using ITServiceDesk.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServiceDesk.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    /// <summary>
    /// Retrieves a filtered, sorted, and paginated list of tickets based on user role.
    /// Employees see only their own tickets; Administrators see all organizational tickets.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<TicketResponseDto>>> GetTickets([FromQuery] TicketFilterDto filter)
    {
        var (userId, userRole) = GetCurrentUser();
        var result = await _ticketService.GetTicketsAsync(filter, userId, userRole);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves full details of a single ticket including comments and audit timeline.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TicketDetailDto>> GetTicketById(int id)
    {
        var (userId, userRole) = GetCurrentUser();
        var ticket = await _ticketService.GetTicketByIdAsync(id, userId, userRole);
        return Ok(ticket);
    }

    /// <summary>
    /// Submits a new support ticket. Automatically generates a ticket number and calculates SLA deadline.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TicketResponseDto>> CreateTicket([FromBody] CreateTicketDto dto)
    {
        var (userId, _) = GetCurrentUser();
        var created = await _ticketService.CreateTicketAsync(dto, userId);
        return CreatedAtAction(nameof(GetTicketById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates ticket core information (Title, Description, Category, Priority).
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TicketResponseDto>> UpdateTicket(int id, [FromBody] UpdateTicketDto dto)
    {
        var (userId, userRole) = GetCurrentUser();
        var updated = await _ticketService.UpdateTicketAsync(id, dto, userId, userRole);
        return Ok(updated);
    }

    /// <summary>
    /// Changes the workflow status of a ticket (Open -> InProgress -> Resolved, etc.).
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TicketResponseDto>> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var (userId, userRole) = GetCurrentUser();
        var updated = await _ticketService.UpdateStatusAsync(id, dto, userId, userRole);
        return Ok(updated);
    }

    /// <summary>
    /// Assigns or reassigns an administrator to handle this ticket (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/assign")]
    public async Task<ActionResult<TicketResponseDto>> AssignTicket(int id, [FromBody] AssignTicketDto dto)
    {
        var (userId, userRole) = GetCurrentUser();
        var updated = await _ticketService.AssignTicketAsync(id, dto, userId, userRole);
        return Ok(updated);
    }

    /// <summary>
    /// Permanently deletes a ticket and its associated comments and history (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var (userId, userRole) = GetCurrentUser();
        await _ticketService.DeleteTicketAsync(id, userId, userRole);
        return NoContent();
    }

    // --- Helper Method to Extract User Claims ---
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
