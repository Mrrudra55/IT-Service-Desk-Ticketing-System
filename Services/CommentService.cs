using ITServiceDesk.Data;
using ITServiceDesk.DTOs.Comments;
using ITServiceDesk.Interfaces;
using ITServiceDesk.Models;
using Microsoft.EntityFrameworkCore;

namespace ITServiceDesk.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _context;

    public CommentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CommentResponseDto>> GetCommentsByTicketIdAsync(
        int ticketId, int currentUserId, string currentUserRole)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket #{ticketId} was not found.");
        }

        // Enforce employee ticket ownership
        if (currentUserRole == "Employee" && ticket.CreatedByUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to view comments on this ticket.");
        }

        return await _context.Comments
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentResponseDto
            {
                Id = c.Id,
                TicketId = c.TicketId,
                UserId = c.UserId,
                AuthorName = c.User != null ? c.User.Name : "Unknown",
                AuthorRole = c.User != null ? c.User.Role : "Employee",
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CommentResponseDto> AddCommentAsync(
        int ticketId, CreateCommentDto dto, int currentUserId, string currentUserRole)
    {
        var ticket = await _context.Tickets
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket #{ticketId} was not found.");
        }

        // Employee can only comment on their own tickets
        if (currentUserRole == "Employee" && ticket.CreatedByUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only add comments to your own tickets.");
        }

        if (ticket.Status == "Closed")
        {
            throw new InvalidOperationException("Comments cannot be added to a closed ticket.");
        }

        var comment = new Comment
        {
            TicketId = ticketId,
            UserId = currentUserId,
            Content = dto.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Comments.AddAsync(comment);

        // Record comment in ticket audit trail
        var history = new TicketHistory
        {
            TicketId = ticketId,
            ChangedByUserId = currentUserId,
            Action = "CommentAdded",
            OldValue = null,
            NewValue = dto.Content.Length > 50 ? dto.Content.Substring(0, 47) + "..." : dto.Content,
            CreatedAt = DateTime.UtcNow
        };
        await _context.TicketHistories.AddAsync(history);

        // Send In-App notification to the other party
        var author = await _context.Users.FindAsync(currentUserId);
        var authorName = author?.Name ?? "Someone";

        if (currentUserRole == "Employee" && ticket.AssignedToUserId.HasValue)
        {
            // Employee commented -> Notify assigned agent
            _context.Notifications.Add(new Notification
            {
                UserId = ticket.AssignedToUserId.Value,
                TicketId = ticket.Id,
                Title = "New Ticket Comment",
                Message = $"{authorName} commented on ticket {ticket.TicketNumber}.",
                Type = "Comment",
                CreatedAt = DateTime.UtcNow
            });
        }
        else if (currentUserRole != "Employee" && ticket.CreatedByUserId != currentUserId)
        {
            // Agent/Admin commented -> Notify employee
            _context.Notifications.Add(new Notification
            {
                UserId = ticket.CreatedByUserId,
                TicketId = ticket.Id,
                Title = "IT Support Reply",
                Message = $"{authorName} posted an update on your ticket {ticket.TicketNumber}.",
                Type = "Comment",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        return new CommentResponseDto
        {
            Id = comment.Id,
            TicketId = comment.TicketId,
            UserId = comment.UserId,
            AuthorName = authorName,
            AuthorRole = author?.Role ?? "Employee",
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }
}

