using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using camera.Data;
using camera.Factories;
using camera.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibVLCSharp.Shared;

namespace camera.ViewModels;

public sealed partial class BoxVm : ObservableObject
{
    [ObservableProperty]
    private double x;

    [ObservableProperty]
    private double y;

    [ObservableProperty]
    private double w;

    [ObservableProperty]
    private double h;

    [ObservableProperty]
    private string? label;

    /// <summary>
    /// Whether this box should be centered (calculated dynamically based on canvas size)
    /// </summary>
    public bool IsCentered { get; set; } = true;

    /// <summary>
    /// Y position for label (above the box)
    /// </summary>
    public double LabelY => Math.Max(Y - 20, 0);
}

/// <summary>
/// ViewModel for polygon overlay (similar to WPF OverlayWindow polygon)
/// </summary>
public sealed partial class PolygonVm : ObservableObject
{
    private Avalonia.Point[]? _points;
    public Avalonia.Point[]? Points
    {
        get => _points;
        set => SetProperty(ref _points, value);
    }

    [ObservableProperty]
    private double x;

    [ObservableProperty]
    private double y;

    [ObservableProperty]
    private double width;

    [ObservableProperty]
    private double height;

    /// <summary>
    /// Safe margin from rectangle
    /// </summary>
    public double SafeMargin { get; set; } = 30;
}

public sealed partial class VideoTileVm : ObservableObject
{
    public required MediaPlayer MediaPlayer { get; init; }

    // Overlay window (similar to WPF OverlayWindow pattern)
    public OverlayWindow? OverlayWindow { get; set; }

    // Reference to the video container element for positioning overlay window
    public Control? VideoContainer { get; set; }
    
    // Overlay config (stored to create overlay when container loads)
    public OverlayConfig? PendingOverlayConfig { get; set; }
}

public partial class HomePageViewModel : PageViewModel
{
    private const int TileCount = 8;

    private readonly MediaFactory _mediaFactory;
    private readonly DispatcherTimer _boxTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    
    // Dictionary tracking overlays by container element (similar to WPF pattern)
    private readonly Dictionary<Control, OverlayWindow> _overlays = new();
    
    // Timer for continuous overlay updates (equivalent to WPF CompositionTarget.Rendering)
    // Reduced frequency to 30fps (33ms) to improve performance
    private readonly DispatcherTimer _overlayUpdateTimer = new() { Interval = TimeSpan.FromMilliseconds(33) }; // ~30fps
    private bool _overlayUpdateHooked = false;
    
    // Timer để tự động di chuyển overlay box mỗi giây
    private readonly DispatcherTimer _overlayPositionTimer = new() { Interval = TimeSpan.FromSeconds(1) };
    private int _overlayPositionIndex = 0;
    
    // Các vị trí để overlay nhảy qua lại
    private readonly (double X, double Y)[] _overlayPositions = new[]
    {
        (0.1, 0.1),   // Góc trên trái
        (0.4, 0.2),   // Trên giữa
        (0.7, 0.1),   // Góc trên phải
        (0.1, 0.5),   // Giữa trái
        (0.4, 0.4),   // Giữa (center)
        (0.7, 0.5),   // Giữa phải
        (0.2, 0.7),   // Dưới trái
        (0.5, 0.7),   // Dưới giữa
        (0.8, 0.7),   // Dưới phải
    };

    // RTSP URL format: rtsp://username:password@ip:port/path
    // Example: rtsp://admin:password123@192.168.1.100:554/Streaming/Channels/101
    // For testing without auth: rtsp://192.168.1.100:554/stream
    // MediaMTX: rtsp://127.0.0.1:8554/hk2mcam
    private readonly string _url = "rtsp://10.2.21.96:8554/hk2mcam";

    public ObservableCollection<VideoTileVm> Tiles { get; }
    public IReadOnlyList<MediaPlayer> MediaPlayers { get; }

    // Legacy: single box for all tiles (kept for backward compat)
    public BoxVm Box { get; } = new BoxVm { X = 40, Y = 30, W = 160, H = 100 };

    public string Test => "Hello from LibVLCSharp";

    public HomePageViewModel(MediaFactory mediaFactory) : base(ApplicationPageNames.Home)
    {
        _mediaFactory = mediaFactory;

        // Subscribe to MediaAdded event to create overlays when media is added
        _mediaFactory.MediaAdded += OnMediaAdded;

        var players = new MediaPlayer[TileCount];
        for (var i = 0; i < TileCount; i++)
            players[i] = _mediaFactory.CreatePlayer();
        MediaPlayers = players;

        Tiles = new ObservableCollection<VideoTileVm>();
        for (var i = 0; i < TileCount; i++)
        {
            var tile = new VideoTileVm { MediaPlayer = MediaPlayers[i] };
            // Overlay boxes will be created automatically when media is added
            Tiles.Add(tile);
        }

        _boxTimer.Tick += (_, _) =>
        {
            Box.X += 20;
            if (Box.X > 400) Box.X = 40;
        };
        _boxTimer.Start();

        // Hook overlay update timer (equivalent to WPF CompositionTarget.Rendering)
        _overlayUpdateTimer.Tick += OnRenderingUpdateOverlays;
        
        // Hook overlay position timer để tự động di chuyển overlay mỗi giây
        _overlayPositionTimer.Tick += OnOverlayPositionTimer;
        _overlayPositionTimer.Start();
        
        // Hook MainWindow property changes to hide/show overlays accordingly
        Dispatcher.UIThread.Post(() =>
        {
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                // Listen to PropertyChanged to detect WindowState changes
                mainWindow.PropertyChanged += (_, e) =>
                {
                    if (e.Property == Window.WindowStateProperty || e.Property == Window.IsVisibleProperty)
                    {
                        // Update all overlays when window state or visibility changes
                        foreach (var kv in _overlays.ToArray())
                        {
                            if (kv.Key != null && kv.Value != null)
                            {
                                UpdateOverlayBoundsForElement(kv.Value, kv.Key);
                            }
                        }
                    }
                };
            }
        }, DispatcherPriority.Loaded);

        // Auto-play all tiles after a short delay
        Dispatcher.UIThread.Post(async () =>
        {
            await Task.Delay(500); // Wait for UI to initialize
            foreach (var tile in Tiles)
            {
                Play(tile.MediaPlayer);
            }
        }, DispatcherPriority.Loaded);
    }
    
    /// <summary>
    /// Handler for continuous overlay updates (equivalent to WPF CompositionTarget.Rendering)
    /// Updates all overlay windows at ~30fps for better performance
    /// </summary>
    private void OnRenderingUpdateOverlays(object? sender, EventArgs e)
    {
        if (_overlays.Count == 0) return; // Skip if no overlays
        
        try
        {
            var invalidOverlays = new List<Control>();
            
            foreach (var kv in _overlays.ToArray())
            {
                // Only update if container is still valid
                if (kv.Key != null && kv.Value != null)
                {
                    // Try to update - UpdateOverlayBoundsForElement will handle if control is not in visual tree
                    try
                    {
                        UpdateOverlayBoundsForElement(kv.Value, kv.Key);
                    }
                    catch (ArgumentException)
                    {
                        // Control is not in visual tree, mark for removal
                        invalidOverlays.Add(kv.Key);
                    }
                }
                else
                {
                    // Null key or value, mark for removal
                    if (kv.Key != null) invalidOverlays.Add(kv.Key);
                }
            }
            
            // Remove invalid overlays to prevent spam
            foreach (var invalidKey in invalidOverlays)
            {
                if (_overlays.TryGetValue(invalidKey, out var overlay))
                {
                    try
                    {
                        overlay?.Hide();
                        overlay?.Close();
                    }
                    catch { /* Ignore errors when closing */ }
                    _overlays.Remove(invalidKey);
                    _lastOverlayBounds.Remove(invalidKey);
                }
            }
        }
        catch (Exception ex)
        {
            // Silently handle errors to prevent timer from stopping
            // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] Error in overlay update: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Handler để tự động di chuyển overlay box sang vị trí khác mỗi giây
    /// </summary>
    private void OnOverlayPositionTimer(object? sender, EventArgs e)
    {
        if (_overlays.Count == 0) return;
        
        try
        {
            // Chuyển sang vị trí tiếp theo
            _overlayPositionIndex = (_overlayPositionIndex + 1) % _overlayPositions.Length;
            var newPosition = _overlayPositions[_overlayPositionIndex];
            
            // Cập nhật tất cả overlays với vị trí mới
            foreach (var overlay in _overlays.Values)
            {
                overlay.BoxPositionNorm = newPosition;
                overlay.UpdateCenteredBox();
            }
            
            // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] 🔄 Overlay moved to position {_overlayPositionIndex}: ({newPosition.X}, {newPosition.Y})");
        }
        catch (Exception ex)
        {
            // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] Error in overlay position update: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Play(MediaPlayer mediaPlayer)
    {
        // Create overlay config (similar to WPF OverlayWindow pattern)
        var overlayConfig = new OverlayConfig
        {
            BoxSizeNorm = (0.15, 0.12), // Normalized size: 15% width, 12% height (matches WPF)
            LabelText = "person",
            Topmost = false,
            CenterBox = true, // Center the box like WPF OverlayWindow
            InitialPositionNorm = (0.4, 0.3), // Only used if CenterBox is false
            ShowPolygon = true, // Show polygon overlay (matches WPF)
            PolygonSizeNorm = (0.15, 0.20), // Normalized polygon size (matches WPF)
            SafeMargin = 30, // Safe margin between rectangle and polygon (matches WPF)
            AutoCreate = true
        };

        // Try software decoding first (more compatible with RTSP)
        // If needed, can switch to hardware decoding later
        // Overlay will be created automatically via MediaAdded event
        _mediaFactory.CreateLiveMedia(_url, mediaPlayer, useHardwareDecoding: false, overlayConfig);
        
        // Check overlay after media is added
        CheckOverlayAfterMediaAdd(mediaPlayer);
    }

    /// <summary>
    /// Handler for when media is added with overlay configuration
    /// This creates OverlayWindow (separate window) that overlays the video tile, similar to WPF pattern
    /// </summary>
    private void OnMediaAdded(object? sender, MediaAddedEventArgs e)
    {
        if (e.OverlayConfig == null || !e.OverlayConfig.AutoCreate)
            return;

        // Find the tile for this media player
        var tile = Tiles.FirstOrDefault(t => ReferenceEquals(t.MediaPlayer, e.MediaPlayer));
        if (tile == null)
        {
            System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] ⚠️ No tile found for media player");
            return;
        }

        // Store overlay config for later use
        tile.PendingOverlayConfig = e.OverlayConfig;

        // Try to create overlay immediately if container is available
        // Otherwise, overlay will be created when container loads (from HomePageView)
        Dispatcher.UIThread.Post(() =>
        {
            CreateOverlayForTile(tile, e.OverlayConfig);
        }, DispatcherPriority.Loaded);

        // Hook into Playing event
        e.MediaPlayer.Playing += (_, _) =>
        {
            // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] ▶️ Video playing - overlay window should be visible");
        };
    }

    /// <summary>
    /// Create overlay window for a tile (called from OnMediaAdded or OnVideoContainerLoaded)
    /// </summary>
    internal void CreateOverlayForTile(VideoTileVm tile, OverlayConfig config)
    {
        // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] 🔧 CreateOverlayForTile called");
        
        if (tile.VideoContainer == null)
        {
            // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] ⚠️ Video container not available yet, overlay will be created when container loads");
            return;
        }

        // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] ✅ Video container found: {tile.VideoContainer.GetType().Name}, IsVisible={tile.VideoContainer.IsVisible}, Bounds={tile.VideoContainer.Bounds}");

        // Close existing overlay window if any
        if (_overlays.TryGetValue(tile.VideoContainer, out var existingOverlay))
        {
            try
            {
                existingOverlay.Close();
            }
            catch (Exception ex)
            {
                // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] Error closing existing overlay: {ex.Message}");
            }
            _overlays.Remove(tile.VideoContainer);
        }

        tile.OverlayWindow?.Close();
        tile.OverlayWindow = null;

        // Create OverlayWindow (separate window, similar to WPF pattern)
        // Note: Topmost = false so overlay doesn't show above other apps, only within our app
        // Overlay will be clipped to MainWindow bounds in UpdateOverlayBoundsForElement
        var overlayWindow = new OverlayWindow
        {
            BoxSizeNorm = (config.BoxSizeNorm.Width, config.BoxSizeNorm.Height),
            LabelText = config.LabelText,
            Topmost = false // False = overlay only shows within app, not above other apps
        };

        // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] ✅ OverlayWindow created (not shown yet)");

        // Store in both dictionary and tile property
        _overlays[tile.VideoContainer] = overlayWindow;
        tile.OverlayWindow = overlayWindow;

        // Start overlay update timer if not already started
        if (!_overlayUpdateHooked)
        {
            _overlayUpdateHooked = true;
            _overlayUpdateTimer.Start();
            // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] ✅ Overlay update timer started");
        }

            // Show overlay window
            overlayWindow.Show();
        // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] ✅ OverlayWindow.Show() called");

        // Update overlay bounds immediately
                UpdateOverlayBoundsForElement(overlayWindow, tile.VideoContainer);

        // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] ✅ OverlayWindow created and configured:");
        // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel]   Label: {config.LabelText}");
        // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel]   BoxSizeNorm: ({config.BoxSizeNorm.Width}, {config.BoxSizeNorm.Height})");
        // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel]   Topmost: {config.Topmost}");
        // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel]   Overlay IsVisible: {overlayWindow.IsVisible}");
        // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel]   Overlay Position: {overlayWindow.Position}");
        // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel]   Overlay Size: {overlayWindow.Width}x{overlayWindow.Height}");
    }

    /// <summary>
    /// Update overlay window bounds to match video container element (similar to WPF UpdateOverlayBoundsForElement)
    /// </summary>
    // Cache last position/size to avoid unnecessary updates
    private readonly Dictionary<Control, (PixelPoint Position, double Width, double Height)> _lastOverlayBounds = new();
    
    /// <summary>
    /// Get MainWindow reference to set as Owner for overlay windows
    /// </summary>
    private Window? GetMainWindow()
    {
        try
        {
            // Try to get MainWindow from Application
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
        }
        catch
        {
            // Ignore errors
        }
        return null;
    }
    
    public void UpdateOverlayBoundsForElement(OverlayWindow overlay, Control element)
    {
        if (overlay == null || element == null) return;

        try
        {
            // Check if element is visible and has valid size (similar to WPF)
            if (!element.IsVisible)
            {
                overlay.Hide();
                return;
            }

            var bounds = element.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                overlay.Hide();
                return;
            }

            // Get MainWindow to ensure overlay stays within app bounds
            var mainWindow = GetMainWindow();
            if (mainWindow == null)
            {
                overlay.Hide();
                return;
            }

            // Try to get screen position - this will throw if element is not in visual tree
            PixelPoint pointToScreen;
            PixelPoint mainWindowScreenPos;
            Rect mainWindowBounds;
            
            try
            {
                pointToScreen = element.PointToScreen(new Point(0, 0));
                mainWindowScreenPos = mainWindow.PointToScreen(new Point(0, 0));
                mainWindowBounds = mainWindow.Bounds;
            }
            catch (ArgumentException)
            {
                // Control is not in visual tree, hide overlay and return silently
                overlay.Hide();
                return;
            }

            // Calculate overlay position relative to element
            var newPosition = pointToScreen;
            var newWidth = Math.Max(1, bounds.Width);
            var newHeight = Math.Max(1, bounds.Height);

            // Calculate MainWindow bounds on screen
            var mainWindowLeft = mainWindowScreenPos.X;
            var mainWindowTop = mainWindowScreenPos.Y;
            var mainWindowRight = mainWindowLeft + mainWindowBounds.Width;
            var mainWindowBottom = mainWindowTop + mainWindowBounds.Height;
            
            // Calculate overlay bounds on screen
            var overlayLeft = newPosition.X;
            var overlayTop = newPosition.Y;
            var overlayRight = overlayLeft + newWidth;
            var overlayBottom = overlayTop + newHeight;
            
            // Check if overlay is completely outside MainWindow - if so, hide it
            if (overlayRight < mainWindowLeft || overlayLeft > mainWindowRight ||
                overlayBottom < mainWindowTop || overlayTop > mainWindowBottom)
            {
                // Overlay is completely outside MainWindow, hide it
                overlay.Hide();
                return;
            }
            
            // Overlay overlaps with MainWindow - clip it to stay within MainWindow bounds
            var clippedLeft = Math.Max(overlayLeft, mainWindowLeft);
            var clippedTop = Math.Max(overlayTop, mainWindowTop);
            var clippedRight = Math.Min(overlayRight, mainWindowRight);
            var clippedBottom = Math.Min(overlayBottom, mainWindowBottom);
            
            // Calculate clipped position and size
            newPosition = new PixelPoint((int)clippedLeft, (int)clippedTop);
            newWidth = Math.Max(1, clippedRight - clippedLeft);
            newHeight = Math.Max(1, clippedBottom - clippedTop);
            
            // Additional check: if overlay is mostly outside MainWindow, hide it
            var overlayArea = newWidth * newHeight;
            var originalArea = bounds.Width * bounds.Height;
            if (overlayArea < originalArea * 0.5) // If less than 50% visible, hide it
            {
                overlay.Hide();
                return;
            }

            // Only update if position or size changed (performance optimization)
            if (_lastOverlayBounds.TryGetValue(element, out var lastBounds))
            {
                if (lastBounds.Position == newPosition && 
                    Math.Abs(lastBounds.Width - newWidth) < 1 && 
                    Math.Abs(lastBounds.Height - newHeight) < 1)
                {
                    // No change, skip update
                    return;
                }
            }

            // Note: In Avalonia, Owner property setter is not accessible
            // Instead, we clip overlay position to MainWindow bounds to ensure it stays within app

            // Set overlay window position and size to match video container
            overlay.Position = newPosition;
            overlay.Width = newWidth;
            overlay.Height = newHeight;

            // Update centered box
            overlay.UpdateCenteredBox();

            // Cache the new bounds
            _lastOverlayBounds[element] = (newPosition, newWidth, newHeight);

            // Show/hide overlay based on MainWindow state
            // Only show if MainWindow is visible and not minimized
            if (!overlay.IsVisible && mainWindow.IsVisible && mainWindow.WindowState != WindowState.Minimized)
            {
                overlay.Show();
            }
            // Hide overlay if MainWindow is minimized or not visible
            else if (overlay.IsVisible && (!mainWindow.IsVisible || mainWindow.WindowState == WindowState.Minimized))
            {
                overlay.Hide();
            }
        }
        catch (Exception ex)
        {
            // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] ❌ Error updating overlay bounds: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Cleanup video resources and overlays (similar to WPF CleanupVideoResources)
    /// </summary>
    public void CleanupVideoResources()
    {
        try
        {
            // Stop overlay update timer
            if (_overlayUpdateHooked)
            {
                _overlayUpdateTimer.Stop();
                _overlayUpdateHooked = false;
            }
            
            // Stop overlay position timer
            _overlayPositionTimer.Stop();

            // Close all overlay windows
            foreach (var overlay in _overlays.Values.ToList())
            {
                try
                {
                    overlay.Close();
                }
                catch (Exception ex)
                {
                    // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] Error closing overlay: {ex.Message}");
                }
            }
            _overlays.Clear();
            _lastOverlayBounds.Clear();

            // Clear overlay references from tiles
            foreach (var tile in Tiles)
            {
                tile.OverlayWindow = null;
                tile.VideoContainer = null;
            }

            System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] ✅ Video resources cleaned up");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] Error cleaning up resources: {ex.Message}");
        }
    }

    private void CheckOverlayAfterMediaAdd(MediaPlayer mediaPlayer)
    {
        // Find the tile for this media player
        var tile = Tiles.FirstOrDefault(t => ReferenceEquals(t.MediaPlayer, mediaPlayer));
        if (tile is null) return;

        // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] Media added for tile, checking overlay...");
        // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] OverlayWindow: {(tile.OverlayWindow != null ? "created" : "null")}");

        // Verify overlay window is initialized
        // if (tile.OverlayWindow != null)
        // {
        //     System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] ✅ OverlayWindow created and ready");
        //     System.Diagnostics.Debug.WriteLine($"[HomePageViewModel]   Label: {tile.OverlayWindow.LabelText}");
        //     System.Diagnostics.Debug.WriteLine($"[HomePageViewModel]   BoxSizeNorm: ({tile.OverlayWindow.BoxSizeNorm.Width}, {tile.OverlayWindow.BoxSizeNorm.Height})");
        // }
        // else
        // {
        //     System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] ⚠️ No OverlayWindow found for this tile");
        // }

        // Hook into Playing event to verify overlay is active when video starts
        mediaPlayer.Playing += (_, _) =>
        {
            // System.Diagnostics.Debug.WriteLine($"[HomePageViewModel] ▶️ Video playing - overlay window should be visible");
        };
    }

    [RelayCommand]
    private void Stop(MediaPlayer mediaPlayer) => mediaPlayer.Stop();
    
    /// <summary>
    /// Cleanup resources - should be called when view is closed or view model is disposed
    /// </summary>
    public void Dispose()
    {
        CleanupVideoResources();
        
        // Unsubscribe from events
        _mediaFactory.MediaAdded -= OnMediaAdded;
        
        // Stop timers
        _boxTimer.Stop();
        _overlayUpdateTimer.Stop();
        _overlayPositionTimer.Stop();
    }
}