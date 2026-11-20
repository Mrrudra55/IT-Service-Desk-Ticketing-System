namespace ITServiceDesk.Models;

public class Attachment
{
    // Unique attachment ID (Primary Key)
    public int Id { get; set; }

    // Associated ticket (Foreign Key)
    public int TicketId { get; set; }

    // Original user-provided file name (e.g. "screenshot.png")
    public string FileName { get; set; } = string.Empty;

    // Secure server-side randomized file name to prevent collision & traversal attacks
    public string StoredFileName { get; set; } = string.Empty;

    // Relative or absolute path where the physical file is stored on the server
    public string FilePath { get; set; } = string.Empty;

    // MIME type (e.g. "image/png", "application/pdf")
    public string ContentType { get; set; } = string.Empty;

    // File size in bytes
    public long FileSize { get; set; }

    // User ID of the uploader (Foreign Key)
    public int UploadedByUserId { get; set; }

    // Timestamp when the file was uploaded
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // --- Navigation Properties ---

    public Ticket? Ticket { get; set; }

    public User? UploadedByUser { get; set; }
}

