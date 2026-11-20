using ITServiceDesk.DTOs.Attachments;

namespace ITServiceDesk.Interfaces;

public interface IAttachmentService
{
    // Uploads and securely stores an attachment on a ticket
    Task<AttachmentDto> UploadAttachmentAsync(int ticketId, IFormFile file, int currentUserId, string currentUserRole);

    // Retrieves file stream and metadata for downloading
    Task<(Stream FileStream, string ContentType, string FileName)> DownloadAttachmentAsync(int attachmentId, int currentUserId, string currentUserRole);

    // Deletes an attachment (uploader or admin)
    Task DeleteAttachmentAsync(int attachmentId, int currentUserId, string currentUserRole);
}

