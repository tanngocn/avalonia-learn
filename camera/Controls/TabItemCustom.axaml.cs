using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using System;

namespace camera.Controls;

public class TabItemCustom : Avalonia.Controls.TabItem
{
    public event EventHandler? CloseRequested;
    private Button? _closeButton;
    private bool _isClosing = false;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        // Find close button and attach click handler
        _closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        if (_closeButton != null)
        {
            // Remove old handlers if any
            _closeButton.PointerPressed -= CloseButton_PointerPressed;
            _closeButton.PointerReleased -= CloseButton_PointerReleased;
            _closeButton.Click -= CloseButton_Click;
            
            // Use PointerPressed to catch early
            _closeButton.PointerPressed += CloseButton_PointerPressed;
            _closeButton.PointerReleased += CloseButton_PointerReleased;
            
            // Also handle Click event as backup
            _closeButton.Click += CloseButton_Click;
            
            // Ensure button can receive events
            _closeButton.IsHitTestVisible = true;
            _closeButton.IsEnabled = true;
            
            System.Diagnostics.Debug.WriteLine($"✅ Close button found and handlers attached for tab: {Header}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"❌ WARNING: PART_CloseButton not found in template for tab: {Header}");
            // Try to find it again after a delay (template might not be fully applied)
            Dispatcher.UIThread.Post(() =>
            {
                _closeButton = this.FindControl<Button>("PART_CloseButton");
                if (_closeButton != null)
                {
                    _closeButton.PointerPressed += CloseButton_PointerPressed;
                    _closeButton.PointerReleased += CloseButton_PointerReleased;
                    _closeButton.Click += CloseButton_Click;
                    _closeButton.IsHitTestVisible = true;
                    _closeButton.IsEnabled = true;
                    System.Diagnostics.Debug.WriteLine($"✅ Close button found on retry for tab: {Header}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Close button still not found on retry for tab: {Header}");
                }
            }, Avalonia.Threading.DispatcherPriority.Loaded);
        }
    }

    private void CloseButton_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isClosing = true;
        e.Handled = true;
        System.Diagnostics.Debug.WriteLine($"🔴 Close button pointer pressed for tab: {Header}");
        // Prevent event from bubbling up to TabItem
        try
        {
            var hasSubscribers = CloseRequested != null;
            System.Diagnostics.Debug.WriteLine($"CloseRequested event has subscribers: {hasSubscribers}");
            CloseRequested?.Invoke(this, EventArgs.Empty);
            System.Diagnostics.Debug.WriteLine($"✅ CloseRequested event invoked for tab: {Header}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ERROR invoking CloseRequested: {ex.Message}");
        }
    }

    private void CloseButton_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isClosing)
        {
            e.Handled = true;
            System.Diagnostics.Debug.WriteLine($"🔴 Close button pointer released for tab: {Header}");
        }
    }

    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _isClosing = true;
        e.Handled = true;
        System.Diagnostics.Debug.WriteLine($"🔴 Close button clicked for tab: {Header}");
        // Prevent event from bubbling up to TabItem
        try
        {
            var hasSubscribers = CloseRequested != null;
            System.Diagnostics.Debug.WriteLine($"CloseRequested event has subscribers: {hasSubscribers}");
            CloseRequested?.Invoke(this, EventArgs.Empty);
            System.Diagnostics.Debug.WriteLine($"✅ CloseRequested event invoked for tab: {Header}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ERROR invoking CloseRequested: {ex.Message}");
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        // If we're closing, don't select tab
        if (_isClosing)
        {
            System.Diagnostics.Debug.WriteLine($"Tab is closing, preventing selection for tab: {Header}");
            e.Handled = true;
            return;
        }
        
        // Check if the click source is the close button or its children
        var source = e.Source;
        if (source != null && _closeButton != null)
        {
            // Check if source is the button itself or a child of the button
            var current = source as Control;
            while (current != null)
            {
                if (current == _closeButton)
                {
                    // Click is on the button, don't select tab - let button handle it
                    System.Diagnostics.Debug.WriteLine($"🔴 Pointer pressed on close button (by source) for tab: {Header}");
                    _isClosing = true;
                    e.Handled = true; // Prevent tab selection
                    // Don't call base - let button handle it
                    return;
                }
                current = current.Parent as Control;
            }
        }
        
        // Also check by position as fallback
        if (_closeButton != null)
        {
            try
            {
                var point = e.GetPosition(_closeButton);
                var bounds = new Rect(0, 0, _closeButton.Bounds.Width, _closeButton.Bounds.Height);
                
                if (bounds.Contains(point))
                {
                    // Click is on the button, don't select tab
                    System.Diagnostics.Debug.WriteLine($"🔴 Pointer pressed on close button area (by position) for tab: {Header}");
                    _isClosing = true;
                    e.Handled = true; // Prevent tab selection
                    // Don't call base - let button handle it
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking close button bounds: {ex.Message}");
            }
        }
        
        // Otherwise, handle normally (select tab)
        base.OnPointerPressed(e);
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        // Reset closing flag after pointer is released
        if (_isClosing)
        {
            _isClosing = false;
            e.Handled = true;
            return;
        }
        
        base.OnPointerReleased(e);
    }
}