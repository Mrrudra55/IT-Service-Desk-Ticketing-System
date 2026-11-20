namespace ITServiceDesk.Models;

public class TicketHistory
{
    // Unique history record ID (Primary Key)
    public int Id { get; set; }

    // Associated ticket being audited (Foreign Key)
    public int TicketId { get; set; }

    // User ID of the person who executed the action (Foreign Key)
    public int ChangedByUserId { get; set; }

    // Action name (e.g. "TicketCreated", "StatusChanged", "Assigned", "PriorityChanged")
    public string Action { get; set; } = string.Empty;

    // Previous value before modification (e.g. "Open")
    public string? OldValue { get; set; }

    // New value after modification (e.g. "InProgress")
    public string? NewValue { get; set; }

    // Timestamp when the change occurred
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation Properties ---

    public Ticket? Ticket { get; set; }

    public User? ChangedByUser { get; set; }
}

