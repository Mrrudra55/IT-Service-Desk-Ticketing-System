namespace ITServiceDesk.Models;

public class User
{
    // Unique ID of the user (Primary Key)
    public int Id { get; set; }

    // User's full name (e.g. "Jane Doe")
    public string Name { get; set; } = string.Empty;

    // User's email address (used for login, must be unique)
    public string Email { get; set; } = string.Empty;

    // Stores the securely hashed password (NEVER plain-text)
    public string PasswordHash { get; set; } = string.Empty;

    // System role: "Employee", "Agent", or "Admin"
    public string Role { get; set; } = "Employee";

    // Allows disabling an account without deleting historical records
    public bool IsActive { get; set; } = true;

    // Account creation timestamp in UTC
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Last update timestamp in UTC
    public DateTime? UpdatedAt { get; set; }

    // --- Navigation Properties (Relationships) ---

    // Tickets submitted by this user
    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

    // Tickets assigned to this user (for IT Agents)
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    // Comments posted by this user
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    // In-app notifications sent to this user
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
