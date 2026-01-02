using Avalonia.Controls;
using camera.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace camera.ViewModels;

public partial class LivePageViewModel() : PageViewModel(ApplicationPageNames.Live)
{
    // Sidebar state
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSidebarVisible))]
    [NotifyPropertyChangedFor(nameof(IsDeviceButtonVisible))]
    private bool _isSidebarOpen = true; // Default mở sidebar

    public bool IsSidebarVisible => IsSidebarOpen;
    public bool IsDeviceButtonVisible => !IsSidebarOpen;

    [RelayCommand]
    private void OpenSidebar() => IsSidebarOpen = true;

    [RelayCommand]
    private void CloseSidebar() => IsSidebarOpen = false;

    /// <summary>
    /// Tạo tab mới từ device node được drag & drop
    /// Trả về thông tin để tạo content, không tạo control instance ngay
    /// </summary>
    public (string TabHeader, string DeviceName, bool IsParent) CreateTabInfoFromDeviceNode(string deviceName, bool isParent)
        => (deviceName, deviceName, isParent);

    /// <summary>
    /// Tạo control content từ thông tin tab
    /// </summary>
    public Control CreateTabContent(string deviceName, bool isParent)
    {
        if (isParent)
        {
            // Node cha → CameraListView
            var listViewModel = new CameraListViewModel(deviceName);
            return new Views.CameraListView
            {
                DataContext = listViewModel
            };
        }

        // Node con → CameraDetailView
        var detailViewModel = new CameraDetailViewModel(deviceName);
        return new Views.CameraDetailView
        {
            DataContext = detailViewModel
        };
    }
}