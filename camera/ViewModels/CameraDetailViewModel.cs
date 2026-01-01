using CommunityToolkit.Mvvm.ComponentModel;

namespace camera.ViewModels;

public partial class CameraDetailViewModel : ViewModelBase
{
    [ObservableProperty]
    private string deviceName = string.Empty;

    [ObservableProperty]
    private string deviceType = "Camera";

    [ObservableProperty]
    private string status = "Active";

    [ObservableProperty]
    private string description = "Device detail information";

    public CameraDetailViewModel(string deviceName)
    {
        DeviceName = deviceName;
        Description = $"Detail view for {deviceName}";
    }
}
