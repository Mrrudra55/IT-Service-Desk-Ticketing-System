using ITServiceDesk.Data;
using ITServiceDesk.DTOs.Notifications;
using ITServiceDesk.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITServiceDesk.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationListDto> GetUserNotificationsAsync(int userId, int limit = 20)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                TicketId = n.TicketId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        var unreadCount = await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        return new NotificationListDto
        {
            Notifications = notifications,
            UnreadCount = unreadCount
        };
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        var unreadList = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (unreadList.Any())
        {
            foreach (var n in unreadList)
            {
                n.IsRead = true;
            }
            await _context.SaveChangesAsync();
        }
    }
}

