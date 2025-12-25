using Avalonia.Controls;
using Avalonia.Interactivity;
using camera.ViewModels;

namespace camera.Views;

public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
    }

    private void OnVideoContainerLoaded(object? sender, RoutedEventArgs e)
    {
        // Store reference to video container and create/update overlay window
        if (sender is Grid container && container.DataContext is VideoTileVm tile && DataContext is HomePageViewModel vm)
        {
            System.Diagnostics.Debug.WriteLine($"[HomePageView] ✅ VideoContainer loaded: {container.GetType().Name}, Bounds={container.Bounds}, IsVisible={container.IsVisible}");
            
            tile.VideoContainer = container;
            
            // Create overlay if config is pending (media was added before container loaded)
            if (tile.OverlayWindow == null && tile.PendingOverlayConfig != null)
            {
                System.Diagnostics.Debug.WriteLine($"[HomePageView] ✅ Creating overlay from pending config");
                vm.CreateOverlayForTile(tile, tile.PendingOverlayConfig);
            }
            // Update overlay window bounds if it exists
            else if (tile.OverlayWindow != null)
            {
                System.Diagnostics.Debug.WriteLine($"[HomePageView] ✅ Updating existing overlay bounds");
                vm.UpdateOverlayBoundsForElement(tile.OverlayWindow, container);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[HomePageView] ⚠️ No overlay to create or update (OverlayWindow={tile.OverlayWindow != null}, PendingConfig={tile.PendingOverlayConfig != null})");
            }
        }
    }

    private void OnVideoContainerSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        // Update overlay window bounds when video container size changes
        // Note: Continuous updates are handled by DispatcherTimer in ViewModel (equivalent to WPF CompositionTarget.Rendering)
        if (sender is Grid container && container.DataContext is VideoTileVm tile && DataContext is HomePageViewModel vm)
        {
            if (tile.OverlayWindow != null)
            {
                vm.UpdateOverlayBoundsForElement(tile.OverlayWindow, container);
            }
        }
    }
    
    private void OnVideoContainerUnloaded(object? sender, RoutedEventArgs e)
    {
        // Cleanup overlay when container is unloaded (similar to WPF container.Unloaded)
        if (sender is Grid container && container.DataContext is VideoTileVm tile && DataContext is HomePageViewModel vm)
        {
            // Remove from dictionary and close overlay
            if (tile.OverlayWindow != null)
            {
                try
                {
                    tile.OverlayWindow.Close();
                }
                catch
                {
                    // Ignore errors during cleanup
                }
                tile.OverlayWindow = null;
            }
            tile.VideoContainer = null;
        }
    }
}