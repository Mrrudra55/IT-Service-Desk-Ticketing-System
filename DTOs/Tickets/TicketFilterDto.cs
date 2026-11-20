namespace ITServiceDesk.DTOs.Tickets;

public class TicketFilterDto
{
    // Search keyword matching TicketNumber, Title, or Description
    public string? Search { get; set; }

    // Filter by specific Category
    public int? CategoryId { get; set; }

    // Filter by Status ("Open", "InProgress", etc.)
    public string? Status { get; set; }

    // Filter by Priority ("Low", "Medium", "High", "Critical")
    public string? Priority { get; set; }

    // Filter by Assigned Agent User ID
    public int? AssignedToUserId { get; set; }

    // Filter: "unassigned" to fetch unassigned tickets queue
    public bool? UnassignedOnly { get; set; }

    // Sorting: "newest", "oldest", "priority", "deadline"
    public string? SortBy { get; set; } = "newest";

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

