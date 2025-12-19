using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media; // IImage

namespace camera.Controls;

public class IconButton : Button
{
    public static readonly StyledProperty<IImage?> IconProperty =
        AvaloniaProperty.Register<IconButton, IImage?>(nameof(Icon));

    public static readonly StyledProperty<int> IconSizeProperty =
        AvaloniaProperty.Register<IconButton, int>(nameof(IconSize));
    
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<IconButton, Orientation>(nameof(DirectionLayout));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public int? IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }
    public Orientation? DirectionLayout
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}