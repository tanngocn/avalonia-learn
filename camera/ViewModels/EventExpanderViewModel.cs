using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using camera.Services;

namespace camera.ViewModels;

public partial class EventExpanderViewModel : ViewModelBase
{
    private readonly IEventService? _eventService;

    [ObservableProperty]
    private ObservableCollection<EventItem> events = new();

    public EventExpanderViewModel(IEventService? eventService = null)
    {
        _eventService = eventService;
        LoadEventsAsync();
    }

    private async void LoadEventsAsync()
    {
        if (_eventService != null)
        {
            var eventList = await _eventService.GetRecentEventsAsync(20);
            Events = new ObservableCollection<EventItem>(eventList);
        }
        else
        {
            // Fallback: Load mock data
            Events = new ObservableCollection<EventItem>
            {
                new EventItem 
                { 
                    Title = "Device Connected", 
                    Description = "Camera 1 has been connected successfully",
                    Timestamp = DateTime.Now.AddMinutes(-5).ToString("HH:mm:ss")
                },
                new EventItem 
                { 
                    Title = "Print Completed", 
                    Description = "Print job #123 has been completed",
                    Timestamp = DateTime.Now.AddMinutes(-10).ToString("HH:mm:ss")
                }
            };
        }
    }
}

public class EventItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
}

