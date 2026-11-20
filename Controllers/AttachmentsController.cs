using System.Security.Claims;
using ITServiceDesk.DTOs.Attachments;
using ITServiceDesk.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServiceDesk.Controllers;

[Authorize]
[ApiController]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    /// <summary>
    /// Uploads a file attachment (screenshot, log file, document) to a ticket.
    /// Max size 10MB. Allowed extensions: png, jpg, jpeg, pdf, txt, log, docx, xlsx, zip.
    /// </summary>
    [HttpPost("api/tickets/{ticketId}/attachments")]
    public async Task<ActionResult<AttachmentDto>> UploadAttachment(int ticketId, IFormFile file)
    {
        var (userId, userRole) = GetCurrentUser();
        var result = await _attachmentService.UploadAttachmentAsync(ticketId, file, userId, userRole);
        return CreatedAtAction(nameof(DownloadAttachment), new { id = result.Id }, result);
    }

    /// <summary>
    /// Downloads or views a ticket file attachment.
    /// </summary>
    [HttpGet("api/attachments/{id}/download")]
    public async Task<IActionResult> DownloadAttachment(int id)
    {
        var (userId, userRole) = GetCurrentUser();
        var (stream, contentType, fileName) = await _attachmentService.DownloadAttachmentAsync(id, userId, userRole);
        return File(stream, contentType, fileName);
    }

    /// <summary>
    /// Deletes a file attachment (Uploader or Admin only).
    /// </summary>
    [HttpDelete("api/attachments/{id}")]
    public async Task<IActionResult> DeleteAttachment(int id)
    {
        var (userId, userRole) = GetCurrentUser();
        await _attachmentService.DeleteAttachmentAsync(id, userId, userRole);
        return NoContent();
    }

    private (int UserId, string UserRole) GetCurrentUser()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(idClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid authentication token.");
        }

        return (userId, roleClaim ?? "Employee");
    }
}

