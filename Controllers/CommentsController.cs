using System.Security.Claims;
using ITServiceDesk.DTOs.Comments;
using ITServiceDesk.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServiceDesk.Controllers;

[Authorize]
[ApiController]
[Route("api/tickets/{ticketId}/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// Retrieves all comments for a specific ticket.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CommentResponseDto>>> GetComments(int ticketId)
    {
        var (userId, userRole) = GetCurrentUser();
        var comments = await _commentService.GetCommentsByTicketIdAsync(ticketId, userId, userRole);
        return Ok(comments);
    }

    /// <summary>
    /// Adds a new comment or update message to a ticket thread.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CommentResponseDto>> AddComment(int ticketId, [FromBody] CreateCommentDto dto)
    {
        var (userId, userRole) = GetCurrentUser();
        var comment = await _commentService.AddCommentAsync(ticketId, dto, userId, userRole);
        return CreatedAtAction(nameof(GetComments), new { ticketId }, comment);
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

