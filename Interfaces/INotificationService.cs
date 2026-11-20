using ITServiceDesk.DTOs.Notifications;

namespace ITServiceDesk.Interfaces;

public interface INotificationService
{
    // Retrieves notifications for the logged in user
    Task<NotificationListDto> GetUserNotificationsAsync(int userId, int limit = 20);

    // Marks a single notification as read
    Task MarkAsReadAsync(int notificationId, int userId);

    // Marks all notifications for a user as read
    Task MarkAllAsReadAsync(int userId);
}

