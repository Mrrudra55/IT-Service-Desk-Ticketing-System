namespace ITServiceDesk.DTOs.Tickets;

public class TicketHistoryDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int ChangedByUserId { get; set; }
    public string ChangedByUserName { get; set; } = string.Empty;
    public string ChangedByUserRole { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; }
}

