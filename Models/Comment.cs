namespace ITServiceDesk.Models;

public class Comment
{
    // Unique comment ID (Primary Key)
    public int Id { get; set; }

    // Ticket to which this comment belongs (Foreign Key)
    public int TicketId { get; set; }

    // User who wrote the comment (Foreign Key)
    public int UserId { get; set; }

    // The text body of the comment
    public string Content { get; set; } = string.Empty;

    // Timestamp when the comment was posted
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation Properties ---

    // Associated ticket
    public Ticket? Ticket { get; set; }

    // Author of the comment
    public User? User { get; set; }
}
