using ITServiceDesk.Data;
using ITServiceDesk.DTOs.Attachments;
using ITServiceDesk.Interfaces;
using ITServiceDesk.Models;
using Microsoft.EntityFrameworkCore;

namespace ITServiceDesk.Services;

public class AttachmentService : IAttachmentService
{
    private readonly AppDbContext _context;
    private readonly string _uploadsDirectory;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".pdf", ".txt", ".log", ".docx", ".xlsx", ".zip"
    };

    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public AttachmentService(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _uploadsDirectory = Path.Combine(environment.ContentRootPath, "Uploads");

        if (!Directory.Exists(_uploadsDirectory))
        {
            Directory.CreateDirectory(_uploadsDirectory);
        }
    }

    public async Task<AttachmentDto> UploadAttachmentAsync(
        int ticketId, IFormFile file, int currentUserId, string currentUserRole)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("No file was provided for upload.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException("File exceeds the maximum permitted size limit of 10 MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File type '{extension}' is not allowed for security reasons.");
        }

        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
        {
            throw new KeyNotFoundException($"Ticket #{ticketId} was not found.");
        }

        // Employee ownership rule
        if (currentUserRole == "Employee" && ticket.CreatedByUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You cannot add attachments to another user's ticket.");
        }

        // Generate safe unique stored file name
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(_uploadsDirectory, storedFileName);

        // Save file to disk
        using (var stream = new FileStream(destinationPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new Attachment
        {
            TicketId = ticketId,
            FileName = Path.GetFileName(file.FileName), // Sanitize
            StoredFileName = storedFileName,
            FilePath = destinationPath,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            FileSize = file.Length,
            UploadedByUserId = currentUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Attachments.AddAsync(attachment);

        // Add to ticket history
        _context.TicketHistories.Add(new TicketHistory
        {
            TicketId = ticketId,
            ChangedByUserId = currentUserId,
            Action = "AttachmentAdded",
            OldValue = null,
            NewValue = attachment.FileName,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var uploader = await _context.Users.FindAsync(currentUserId);

        return new AttachmentDto
        {
            Id = attachment.Id,
            TicketId = attachment.TicketId,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            UploadedByUserId = attachment.UploadedByUserId,
            UploadedByUserName = uploader?.Name ?? "Unknown",
            CreatedAt = attachment.CreatedAt
        };
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadAttachmentAsync(
        int attachmentId, int currentUserId, string currentUserRole)
    {
        var attachment = await _context.Attachments
            .Include(a => a.Ticket)
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        if (attachment == null)
        {
            throw new KeyNotFoundException($"Attachment #{attachmentId} was not found.");
        }

        // Ownership access check
        if (currentUserRole == "Employee" && attachment.Ticket?.CreatedByUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to download this attachment.");
        }

        if (!File.Exists(attachment.FilePath))
        {
            throw new FileNotFoundException("The requested file was not found on the server storage.");
        }

        var stream = new FileStream(attachment.FilePath, FileMode.Open, FileAccess.Read);
        return (stream, attachment.ContentType, attachment.FileName);
    }

    public async Task DeleteAttachmentAsync(int attachmentId, int currentUserId, string currentUserRole)
    {
        var attachment = await _context.Attachments
            .Include(a => a.Ticket)
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        if (attachment == null)
        {
            throw new KeyNotFoundException($"Attachment #{attachmentId} was not found.");
        }

        // Only uploader or Admin can delete
        if (currentUserRole != "Admin" && attachment.UploadedByUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this attachment.");
        }

        if (File.Exists(attachment.FilePath))
        {
            try { File.Delete(attachment.FilePath); } catch { /* ignore */ }
        }

        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();
    }
}

