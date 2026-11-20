using ITServiceDesk.DTOs.Comments;

namespace ITServiceDesk.Interfaces;

public interface ICommentService
{
    // Retrieve all comments for an authorized ticket
    Task<List<CommentResponseDto>> GetCommentsByTicketIdAsync(int ticketId, int currentUserId, string currentUserRole);

    // Add a comment to an authorized ticket and notify relevant participants
    Task<CommentResponseDto> AddCommentAsync(int ticketId, CreateCommentDto dto, int currentUserId, string currentUserRole);
}

