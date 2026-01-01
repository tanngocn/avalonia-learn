using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using camera.Services;

namespace camera.ViewModels;

public partial class DeviceExpanderViewModel : ViewModelBase
{
    private readonly IDeviceService? _deviceService;

    [ObservableProperty]
    private ObservableCollection<DeviceNode> devices = new();

    public DeviceExpanderViewModel(IDeviceService? deviceService = null)
    {
        _deviceService = deviceService;
        // Load mock data ngay lập tức
        LoadMockData();
        // Sau đó load từ service nếu có
        if (_deviceService != null)
        {
            LoadDevicesAsync();
        }
    }

    private void LoadMockData()
    {
        // Load mock data ngay lập tức để hiển thị
        Devices = new ObservableCollection<DeviceNode>
            {
                new DeviceNode 
                { 
                    Name = "Camera 1", 
                    Children = new ObservableCollection<DeviceNode>
                    {
                        new DeviceNode { Name = "Sensor 1" },
                        new DeviceNode { Name = "Sensor 2" },
                        new DeviceNode 
                        { 
                            Name = "Lens",
                            Children = new ObservableCollection<DeviceNode>
                            {
                                new DeviceNode { Name = "Auto Focus" },
                                new DeviceNode { Name = "Zoom Control" }
                            }
                        }
                    }
                },
                new DeviceNode 
                { 
                    Name = "Camera 2", 
                    Children = new ObservableCollection<DeviceNode>
                    {
                        new DeviceNode { Name = "Sensor 1" },
                        new DeviceNode { Name = "Sensor 2" },
                        new DeviceNode { Name = "Sensor 3" }
                    }
                },
                new DeviceNode 
                { 
                    Name = "Printer 1",
                    Children = new ObservableCollection<DeviceNode>
                    {
                        new DeviceNode { Name = "Paper Tray 1" },
                        new DeviceNode { Name = "Paper Tray 2" },
                        new DeviceNode 
                        { 
                            Name = "Ink Cartridges",
                            Children = new ObservableCollection<DeviceNode>
                            {
                                new DeviceNode { Name = "Black" },
                                new DeviceNode { Name = "Cyan" },
                                new DeviceNode { Name = "Magenta" },
                                new DeviceNode { Name = "Yellow" }
                            }
                        }
                    }
                },
                new DeviceNode { Name = "Scanner 1" },
                new DeviceNode 
                { 
                    Name = "Network Devices",
                    Children = new ObservableCollection<DeviceNode>
                    {
                        new DeviceNode { Name = "Router" },
                        new DeviceNode { Name = "Switch" },
                        new DeviceNode 
                        { 
                            Name = "Access Points",
                            Children = new ObservableCollection<DeviceNode>
                            {
                                new DeviceNode { Name = "AP-01" },
                                new DeviceNode { Name = "AP-02" }
                            }
                        }
                    }
                }
            };
    }

    private async void LoadDevicesAsync()
    {
        if (_deviceService == null) return;
        
        try
        {
            await Task.Delay(100); // Simulate async
            var deviceList = await _deviceService.GetDevicesAsync();
            Devices = new ObservableCollection<DeviceNode>(deviceList);
        }
        catch
        {
            // Keep mock data if service fails
            System.Diagnostics.Debug.WriteLine("Failed to load devices from service, using mock data");
        }
    }
}

public partial class DeviceNode : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<DeviceNode>? _children;
}

