namespace camera.Data;

/// <summary>
/// Configuration for overlay boxes displayed on video tiles
/// Similar to WPF OverlayWindow pattern
/// </summary>
public class OverlayConfig
{
    /// <summary>
    /// Normalized box size (0.0 to 1.0) relative to video dimensions
    /// </summary>
    public (double Width, double Height) BoxSizeNorm { get; set; } = (0.15, 0.12);

    /// <summary>
    /// Default label text for overlay boxes
    /// </summary>
    public string LabelText { get; set; } = "person";

    /// <summary>
    /// Whether overlay should be topmost (always on top)
    /// </summary>
    public bool Topmost { get; set; } = false;

    /// <summary>
    /// Whether to center the box (if true, InitialPositionNorm is ignored)
    /// </summary>
    public bool CenterBox { get; set; } = true;

    /// <summary>
    /// Initial box position (normalized, 0.0 to 1.0) - only used if CenterBox is false
    /// </summary>
    public (double X, double Y) InitialPositionNorm { get; set; } = (0.4, 0.3);

    /// <summary>
    /// Whether to show polygon overlay (similar to WPF OverlayWindow)
    /// </summary>
    public bool ShowPolygon { get; set; } = true;

    /// <summary>
    /// Normalized polygon size (0.0 to 1.0) relative to video dimensions
    /// </summary>
    public (double Width, double Height) PolygonSizeNorm { get; set; } = (0.15, 0.20);

    /// <summary>
    /// Safe margin between rectangle and polygon (in pixels)
    /// </summary>
    public double SafeMargin { get; set; } = 30;

    /// <summary>
    /// Whether to create overlay automatically when media is added
    /// </summary>
    public bool AutoCreate { get; set; } = true;
}

