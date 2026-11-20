namespace ITServiceDesk.Models;

public class Notification
{
    // Unique notification ID (Primary Key)
    public int Id { get; set; }

    // Recipient of the notification (Foreign Key)
    public int UserId { get; set; }

    // Optional ticket reference associated with this notification
    public int? TicketId { get; set; }

    // Notification title (e.g. "Ticket Assigned")
    public string Title { get; set; } = string.Empty;

    // Notification message body (e.g. "Agent Smith has been assigned to ticket TCK-202611-0001")
    public string Message { get; set; } = string.Empty;

    // Category of event (e.g. "Assignment", "StatusChange", "Comment", "System")
    public string Type { get; set; } = "System";

    // Read/unread flag
    public bool IsRead { get; set; } = false;

    // Notification creation timestamp in UTC
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation Properties ---

    public User? User { get; set; }

    public Ticket? Ticket { get; set; }
}

