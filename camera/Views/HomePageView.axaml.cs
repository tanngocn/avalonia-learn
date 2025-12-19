using System;
using Avalonia.Controls;
using Avalonia.Threading;
using camera.ViewModels;

namespace camera.Views;

public partial class HomePageView : UserControl
{
    private HomePageViewModel? _vm;

    public HomePageView()
    {
        InitializeComponent();

        DataContextChanged += (_, _) => WireVm();
        AttachedToVisualTree += (_, _) => WireVm();
        DetachedFromVisualTree += (_, _) => UnwireVm();
    }

    private void WireVm()
    {
        UnwireVm();

        _vm = DataContext as HomePageViewModel;
        if (_vm is null) return;

        _vm.FrameUpdated += OnFrameUpdated;
    }

    private void UnwireVm()
    {
        if (_vm is null) return;
        _vm.FrameUpdated -= OnFrameUpdated;
        _vm = null;
    }

    private void OnFrameUpdated()
    {
        // VideoImage là Image có x:Name="VideoImage" trong XAML
        Dispatcher.UIThread.Post(() => VideoImage.InvalidateVisual(), DispatcherPriority.Render);
        Dispatcher.UIThread.Post(() => VideoImage1.InvalidateVisual(), DispatcherPriority.Render);
        Dispatcher.UIThread.Post(() => VideoImage2.InvalidateVisual(), DispatcherPriority.Render);
        Dispatcher.UIThread.Post(() => VideoImage3.InvalidateVisual(), DispatcherPriority.Render);
    }
}