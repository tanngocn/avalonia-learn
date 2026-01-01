using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace camera.ViewModels;

public partial class CameraListViewModel : ViewModelBase
{
    [ObservableProperty]
    private string deviceName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CameraItem> cameras = new();

    public CameraListViewModel(string deviceName)
    {
        DeviceName = deviceName;
        LoadCameras();
    }

    private void LoadCameras()
    {
        // Mock data - sẽ load từ service sau
        Cameras = new ObservableCollection<CameraItem>
        {
            new CameraItem { Name = "Camera 1", Description = "Main camera for device" },
            new CameraItem { Name = "Camera 2", Description = "Secondary camera" },
            new CameraItem { Name = "Camera 3", Description = "Backup camera" }
        };
    }
}

public class CameraItem
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
