# Hướng Dẫn Sidebar Expanders - Tách Nhỏ Services

Guide này giải thích cách tách 3 Expander trong Sidebar thành các UserControl riêng với services riêng.

## 📋 Cấu Trúc

```
Sidebar
├── DeviceExpanderView (TreeView - thiết bị)
│   ├── DeviceExpanderViewModel
│   └── IDeviceService
├── EventExpanderView (Danh sách events)
│   ├── EventExpanderViewModel
│   └── IEventService
└── NotificationExpanderView (Thông báo)
    ├── NotificationExpanderViewModel
    └── INotificationService
```

---

## 📁 Files Đã Tạo

### Views:
1. `Views/DeviceExpanderView.axaml` - Expander với TreeView
2. `Views/EventExpanderView.axaml` - Expander với danh sách events
3. `Views/NotificationExpanderView.axaml` - Expander với notifications

### ViewModels:
1. `ViewModels/DeviceExpanderViewModel.cs`
2. `ViewModels/EventExpanderViewModel.cs`
3. `ViewModels/NotificationExpanderViewModel.cs`

### Services (Interfaces):
1. `Services/IDeviceService.cs`
2. `Services/IEventService.cs`
3. `Services/INotificationService.cs`

---

## 🔧 Implement Services

### 1. DeviceService Implementation

Tạo file `Services/DeviceService.cs`:

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using camera.ViewModels;

namespace camera.Services;

public class DeviceService : IDeviceService
{
    public async Task<List<DeviceNode>> GetDevicesAsync()
    {
        // TODO: Call API hoặc đọc từ database
        await Task.Delay(100); // Simulate async
        
        return new List<DeviceNode>
        {
            new DeviceNode { Name = "Camera 1", Children = new ObservableCollection<DeviceNode>
            {
                new DeviceNode { Name = "Sensor 1" },
                new DeviceNode { Name = "Sensor 2" }
            }},
            new DeviceNode { Name = "Printer 1" }
        };
    }

    public async Task<DeviceNode?> GetDeviceByIdAsync(string deviceId)
    {
        // Implementation
        await Task.CompletedTask;
        return null;
    }

    public async Task<bool> ConnectDeviceAsync(string deviceId)
    {
        // Implementation
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> DisconnectDeviceAsync(string deviceId)
    {
        // Implementation
        await Task.CompletedTask;
        return true;
    }
}
```

### 2. EventService Implementation

Tạo file `Services/EventService.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using camera.ViewModels;

namespace camera.Services;

public class EventService : IEventService
{
    private readonly List<EventItem> _events = new();

    public async Task<List<EventItem>> GetEventsAsync()
    {
        await Task.CompletedTask;
        return new List<EventItem>(_events);
    }

    public async Task<List<EventItem>> GetRecentEventsAsync(int count = 10)
    {
        await Task.CompletedTask;
        return _events.Take(count).ToList();
    }

    public async Task AddEventAsync(EventItem eventItem)
    {
        _events.Insert(0, eventItem);
        await Task.CompletedTask;
    }
}
```

### 3. NotificationService Implementation

Tạo file `Services/NotificationService.cs`:

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using camera.ViewModels;

namespace camera.Services;

public class NotificationService : INotificationService
{
    private readonly List<NotificationItem> _notifications = new();

    public async Task<List<NotificationItem>> GetNotificationsAsync()
    {
        await Task.CompletedTask;
        return new List<NotificationItem>(_notifications);
    }

    public async Task AddNotificationAsync(string message)
    {
        _notifications.Insert(0, new NotificationItem
        {
            Message = message,
            Time = DateTime.Now.ToString("HH:mm")
        });
        await Task.CompletedTask;
    }

    public async Task DismissNotificationAsync(NotificationItem notification)
    {
        _notifications.Remove(notification);
        await Task.CompletedTask;
    }

    public async Task ClearAllNotificationsAsync()
    {
        _notifications.Clear();
        await Task.CompletedTask;
    }
}
```

---

## ⚙️ Đăng Ký Services trong DI

Cập nhật `App.axaml.cs`:

```csharp
// Đăng ký Services
collection.AddScoped<IDeviceService, DeviceService>();
collection.AddScoped<IEventService, EventService>();
collection.AddScoped<INotificationService, NotificationService>();

// Đăng ký ViewModels
collection.AddTransient<DeviceExpanderViewModel>();
collection.AddTransient<EventExpanderViewModel>();
collection.AddTransient<NotificationExpanderViewModel>();
```

---

## 📝 Sử Dụng trong Views

Views sẽ tự động inject ViewModels từ DI container. Nếu cần inject thủ công:

```xml
<UserControl>
    <UserControl.DataContext>
        <vm:DeviceExpanderViewModel />
    </UserControl.DataContext>
    <!-- ... -->
</UserControl>
```

---

## 🎯 Lợi Ích

✅ **Tách biệt concerns**: Mỗi expander có service riêng
✅ **Dễ test**: Có thể mock services
✅ **Dễ maintain**: Code được tổ chức rõ ràng
✅ **Scalable**: Dễ thêm features mới

---

## 📚 Next Steps

1. Implement các services với logic thực tế
2. Thêm error handling
3. Thêm loading states
4. Thêm real-time updates (nếu cần)

