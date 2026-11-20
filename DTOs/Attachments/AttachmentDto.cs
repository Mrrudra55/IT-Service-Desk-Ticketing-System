namespace ITServiceDesk.DTOs.Attachments;

public class AttachmentDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FormattedSize => FileSize < 1024 * 1024 
        ? $"{Math.Round((double)FileSize / 1024, 1)} KB" 
        : $"{Math.Round((double)FileSize / (1024 * 1024), 1)} MB";
    public int UploadedByUserId { get; set; }
    public string UploadedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

