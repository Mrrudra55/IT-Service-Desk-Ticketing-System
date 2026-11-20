namespace ITServiceDesk.Models;

public class Ticket
{
    // Unique database primary key
    public int Id { get; set; }

    // Human-readable ticket reference code (e.g. "TCK-202611-0001")
    public string TicketNumber { get; set; } = string.Empty;

    // Short summary of the reported technical issue
    public string Title { get; set; } = string.Empty;

    // Comprehensive description of the issue
    public string Description { get; set; } = string.Empty;

    // Category identifier (Foreign Key to TicketCategory)
    public int CategoryId { get; set; }

    // Current ticket workflow status: "Open", "InProgress", "OnHold", "Resolved", "Closed"
    public string Status { get; set; } = "Open";

    // Urgency level: "Low", "Medium", "High", "Critical"
    public string Priority { get; set; } = "Medium";

    // User ID of the employee who reported the ticket (Foreign Key to User)
    public int CreatedByUserId { get; set; }

    // User ID of the IT agent assigned to resolve the ticket (Nullable Foreign Key)
    public int? AssignedToUserId { get; set; }

    // Timestamp when the ticket was submitted
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Timestamp of the latest modification
    public DateTime? UpdatedAt { get; set; }

    // Expected resolution deadline based on SLA policy
    public DateTime? DueAt { get; set; }

    // Timestamp when ticket was transitioned to "Resolved"
    public DateTime? ResolvedAt { get; set; }

    // --- Navigation Properties ---

    // Related category object
    public TicketCategory? Category { get; set; }

    // The employee who submitted this ticket
    public User? CreatedByUser { get; set; }

    // The IT agent currently assigned (null if unassigned)
    public User? AssignedToUser { get; set; }

    // Comments and communication thread for this ticket
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    // Audit trail tracking all status changes, reassignments, etc.
    public ICollection<TicketHistory> Histories { get; set; } = new List<TicketHistory>();

    // Supporting file attachments (screenshots, logs, diagnostics)
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
