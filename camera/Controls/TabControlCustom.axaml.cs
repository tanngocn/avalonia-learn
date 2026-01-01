using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace camera.Controls;

public class TabControlCustom : Avalonia.Controls.TabControl
{
    // Override to ensure TabItem.Content is always null when container is prepared
    // This prevents TabControl from trying to use it for SelectedContent
    // We manage content separately in TabContentControl (see LivePageView)
    protected override void PrepareContainerForItemOverride(Control element, object? item, int index)
    {
        base.PrepareContainerForItemOverride(element, item, index);
        
        // If container is a TabItem, ensure its Content is null to prevent SelectedContent conflicts
        // We store tab info in Tag and create content dynamically in LivePageView.OnTabChanged
        if (element is TabItem tabItem && tabItem.Content != null)
        {
            // Clear Content - we store info in Tag and create content dynamically
            tabItem.Content = null;
        }
        
        // Note: CloseRequested event subscription is handled in LivePageView after tab is added
        // This ensures the handler is properly attached to the container instance
    }
}