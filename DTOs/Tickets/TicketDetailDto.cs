using ITServiceDesk.DTOs.Attachments;
using ITServiceDesk.DTOs.Comments;

namespace ITServiceDesk.DTOs.Tickets;

public class TicketDetailDto : TicketResponseDto
{
    // Full thread of comments ordered chronologically
    public List<CommentResponseDto> Comments { get; set; } = new List<CommentResponseDto>();

    // Complete audit timeline of changes made to this ticket
    public List<TicketHistoryDto> Histories { get; set; } = new List<TicketHistoryDto>();

    // File attachments (screenshots, logs, diagnostic reports)
    public List<AttachmentDto> Attachments { get; set; } = new List<AttachmentDto>();
}
