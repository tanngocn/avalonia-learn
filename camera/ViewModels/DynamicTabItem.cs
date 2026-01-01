using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace camera.ViewModels;

public partial class DynamicTabItem : ObservableObject
{
    [ObservableProperty]
    private string header = string.Empty;

    [ObservableProperty]
    private string deviceName = string.Empty;

    [ObservableProperty]
    private bool isParent;

    [ObservableProperty]
    private bool isSelected;

    public Control? Content { get; set; }
}

