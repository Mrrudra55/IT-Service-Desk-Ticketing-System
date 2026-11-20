namespace ITServiceDesk.Models;

public class TicketCategory
{
    // Unique ID of the category
    public int Id { get; set; }

    // Category name (e.g., "Hardware", "Software", "Network")
    public string Name { get; set; } = string.Empty;

    // Optional short description of what issues belong here
    public string Description { get; set; } = string.Empty;

    // Enables soft-disabling a category without breaking existing tickets
    public bool IsActive { get; set; } = true;

    // All tickets categorized under this category
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}

