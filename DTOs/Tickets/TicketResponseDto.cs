namespace ITServiceDesk.DTOs.Tickets;

public class TicketResponseDto
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;

    // Category Details
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    // Submitter Details
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public string CreatedByUserEmail { get; set; } = string.Empty;

    // Assigned IT Agent Details
    public int? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // SLA Helper: True if deadline passed and ticket isn't resolved/closed
    public bool IsOverdue => DueAt.HasValue && 
                             DueAt.Value < DateTime.UtcNow && 
                             Status != "Resolved" && 
                             Status != "Closed";

    // Summary metadata
    public int CommentsCount { get; set; }
}

