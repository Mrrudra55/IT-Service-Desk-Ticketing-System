using System.ComponentModel.DataAnnotations;

namespace ITServiceDesk.DTOs.Tickets;

public class AssignTicketDto
{
    // Pass null or 0 to unassign the ticket
    public int? AssignedToUserId { get; set; }
}

public class UpdateStatusDto
{
    [Required]
    [RegularExpression("^(Open|InProgress|OnHold|Resolved|Closed)$", 
        ErrorMessage = "Status must be Open, InProgress, OnHold, Resolved, or Closed.")]
    public string Status { get; set; } = "Open";
}

