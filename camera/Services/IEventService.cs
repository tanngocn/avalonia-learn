using System.Collections.Generic;
using System.Threading.Tasks;
using camera.ViewModels;

namespace camera.Services;

/// <summary>
/// Service để quản lý events
/// </summary>
public interface IEventService
{
    Task<List<EventItem>> GetEventsAsync();
    Task<List<EventItem>> GetRecentEventsAsync(int count = 10);
    Task AddEventAsync(EventItem eventItem);
}

