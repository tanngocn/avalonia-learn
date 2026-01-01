using System.Collections.Generic;
using System.Threading.Tasks;
using camera.ViewModels;

namespace camera.Services;

/// <summary>
/// Service để quản lý notifications
/// </summary>
public interface INotificationService
{
    Task<List<NotificationItem>> GetNotificationsAsync();
    Task AddNotificationAsync(string message);
    Task DismissNotificationAsync(NotificationItem notification);
    Task ClearAllNotificationsAsync();
}

