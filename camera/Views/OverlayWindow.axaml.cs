using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using camera.Data;

namespace camera.Views;

/// <summary>
/// Cửa sổ overlay trong suốt, vẽ 1 bounding box + nhãn ở giữa và polygon ở góc.
/// Luôn đè lên tile video (airspace-safe).
/// Tương tự WPF OverlayWindow pattern.
/// </summary>
public partial class OverlayWindow : Window
{
    private readonly Rectangle _rect;
    private readonly TextBlock _label;
    private readonly Polygon _poly;

    /// <summary>Kích thước box theo tỷ lệ (0..1) của overlay: Width, Height.</summary>
    public (double Width, double Height) BoxSizeNorm { get; set; } = (0.15, 0.12);

    /// <summary>Vị trí box theo tỷ lệ (0..1) của overlay: X, Y. Null = center.</summary>
    public (double X, double Y)? BoxPositionNorm { get; set; } = null;

    /// <summary>Nhãn hiển thị.</summary>
    public string LabelText
    {
        get => _label.Text ?? string.Empty;
        set => _label.Text = value;
    }

    public OverlayWindow()
    {
        InitializeComponent();
        
        // Get references to controls
        _rect = this.FindControl<Rectangle>("RectBox") ?? throw new InvalidOperationException("RectBox not found");
        _label = this.FindControl<TextBlock>("LabelTextBlock") ?? throw new InvalidOperationException("LabelTextBlock not found");
        _poly = this.FindControl<Polygon>("PolygonShape") ?? throw new InvalidOperationException("PolygonShape not found");

        // Update when window size changes (similar to WPF OverlayWindow.SizeChanged)
        this.SizeChanged += (_, _) => UpdateCenteredBox();
    }

    /// <summary>
    /// Cập nhật vị trí/kích thước box & label để nằm giữa overlay.
    /// Tương tự WPF OverlayWindow.UpdateCenteredBox()
    /// </summary>
    public void UpdateCenteredBox()
    {
        double cw = Width; // Use Width (Avalonia equivalent of ActualWidth)
        double ch = Height; // Use Height (Avalonia equivalent of ActualHeight)
        if (cw <= 0 || ch <= 0) return;

        // ===== rectangle - có thể center hoặc custom position =====
        double bw = BoxSizeNorm.Width * cw;
        double bh = BoxSizeNorm.Height * ch;
        
        double x, y;
        if (BoxPositionNorm.HasValue)
        {
            // Custom position
            x = BoxPositionNorm.Value.X * cw;
            y = BoxPositionNorm.Value.Y * ch;
        }
        else
        {
            // Center position
            x = (cw - bw) / 2.0;
            y = (ch - bh) / 2.0;
        }

        _rect.Width = bw;
        _rect.Height = bh;
        Canvas.SetLeft(_rect, x);
        Canvas.SetTop(_rect, y);

        _label.Text = LabelText;
        Canvas.SetLeft(_label, x);
        Canvas.SetTop(_label, Math.Max(y - (_label.FontSize + 6), 0));

        // ============= POLYGON Ở GẦN GIỮA (GÓC TRÊN BÊN PHẢI) =============

        // Khoảng cách an toàn tối thiểu giữa Polygon và Rectangle
        double safeMargin = 30;
        // Kích thước mong muốn của Polygon (tùy chỉnh)
        double polyWidth = 0.15 * cw;
        double polyHeight = 0.20 * ch;

        // Tọa độ điểm bắt đầu (trên bên phải)
        // Đặt Polygon ở bên phải của Rectangle và cách một khoảng `safeMargin`
        double polyStartX = x + bw + safeMargin;
        // Đặt Polygon ở phía trên của Rectangle và cách một khoảng `safeMargin` (hoặc đặt gần mép trên)
        double polyStartY = y - polyHeight - safeMargin;

        // Đảm bảo Polygon không vượt ra ngoài mép cửa sổ
        if (polyStartX + polyWidth > cw)
        {
            polyStartX = cw - polyWidth;
        }
        if (polyStartY < 0)
        {
            polyStartY = safeMargin; // Đặt cách mép trên một chút
        }

        Canvas.SetLeft(_poly, polyStartX);
        Canvas.SetTop(_poly, polyStartY);

        // Create polygon points (similar to WPF OverlayWindow - using PointCollection equivalent)
        var points = new List<Point>
        {
            // Góc trên bên trái của Polygon
            new Point(polyStartX, polyStartY),
            // Góc trên bên phải của Polygon (điểm này có thể tạo hình xiên)
            new Point(polyStartX + polyWidth * 0.9, polyStartY + polyHeight * 0.1),
            // Góc dưới bên phải (hình xiên nhẹ)
            new Point(polyStartX + polyWidth, polyStartY + polyHeight),
            // Góc dưới bên trái
            new Point(polyStartX + polyWidth * 0.1, polyStartY + polyHeight * 0.8)
        };
        _poly.Points = points;
    }
}

