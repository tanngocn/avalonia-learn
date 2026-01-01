using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using camera.Services;

namespace camera.ViewModels;

public partial class NotificationExpanderViewModel : ViewModelBase
{
    private readonly INotificationService? _notificationService;

    [ObservableProperty]
    private ObservableCollection<NotificationItem> notifications = new();

    public NotificationExpanderViewModel(INotificationService? notificationService = null)
    {
        _notificationService = notificationService;
        LoadNotificationsAsync();
    }

    private async void LoadNotificationsAsync()
    {
        if (_notificationService != null)
        {
            var notificationList = await _notificationService.GetNotificationsAsync();
            Notifications = new ObservableCollection<NotificationItem>(notificationList);
        }
        else
        {
            // Fallback: Load mock data
            Notifications = new ObservableCollection<NotificationItem>
            {
                new NotificationItem 
                { 
                    Message = "New device detected",
                    Time = DateTime.Now.AddMinutes(-2).ToString("HH:mm")
                },
                new NotificationItem 
                { 
                    Message = "System update available",
                    Time = DateTime.Now.AddMinutes(-15).ToString("HH:mm")
                }
            };
        }
    }

    [RelayCommand]
    private async Task DismissAsync(NotificationItem? notification)
    {
        if (notification == null) return;

        if (_notificationService != null)
        {
            await _notificationService.DismissNotificationAsync(notification);
        }

        Notifications.Remove(notification);
    }
}

public class NotificationItem
{
    public string Message { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
}

