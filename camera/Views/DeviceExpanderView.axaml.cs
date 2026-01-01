using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using camera.ViewModels;

namespace camera.Views;

public partial class DeviceExpanderView : UserControl
{
    private TreeView? _deviceTreeView;
    private Point _pointerPressedPosition;
    private bool _isDragging;
    private TreeViewItem? _draggingItem;
    private const double DragThreshold = 5.0; // Minimum distance to start drag

    public DeviceExpanderView()
    {
        InitializeComponent();
        // Set DataContext nếu chưa có
        if (DataContext == null)
        {
            var viewModel = new DeviceExpanderViewModel();
            DataContext = viewModel;
            System.Diagnostics.Debug.WriteLine($"DeviceExpanderView: Created ViewModel with {viewModel.Devices.Count} devices");
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        
        // Find TreeView and subscribe to container prepared to attach handlers to TreeViewItems
        _deviceTreeView = this.FindControl<TreeView>("DeviceTreeView");
        if (_deviceTreeView != null)
        {
            _deviceTreeView.ContainerPrepared += TreeView_ContainerPrepared;
        }
    }
    
    private void TreeView_ContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        // Attach drag handlers to each TreeViewItem when it's created
        if (e.Container is TreeViewItem item)
        {
            // Add style class for TreeViewItem
            item.Classes.Add("DeviceTreeViewItemStyle");
            
            System.Diagnostics.Debug.WriteLine($"ContainerPrepared: Attaching handlers to TreeViewItem");
            item.PointerPressed += TreeViewItem_PointerPressed;
            item.PointerMoved += TreeViewItem_PointerMoved;
            item.PointerReleased += TreeViewItem_PointerReleased;
        }
    }
    
    private void TreeViewItem_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        // Find the TreeViewItem at the click position
        // This ensures we get the correct TreeViewItem (child or parent) that was actually clicked
        var clickPosition = e.GetPosition(this);
        var hitTestResult = this.InputHitTest(clickPosition);
        
        // Walk up the visual tree to find the TreeViewItem
        TreeViewItem? clickedItem = null;
        var current = hitTestResult as Visual;
        while (current != null)
        {
            if (current is TreeViewItem item && item.DataContext is DeviceNode)
            {
                clickedItem = item;
                break;
            }
            current = current.GetVisualParent();
        }
        
        // Fallback to sender if we couldn't find one
        if (clickedItem == null && sender is TreeViewItem senderItem)
        {
            clickedItem = senderItem;
        }

        if (clickedItem != null && clickedItem.DataContext is DeviceNode deviceNode)
        {
            System.Diagnostics.Debug.WriteLine($"========== POINTER PRESSED ==========");
            System.Diagnostics.Debug.WriteLine($"TreeViewItem_PointerPressed: {deviceNode.Name}");
            System.Diagnostics.Debug.WriteLine($"IsParent: {deviceNode.Children != null && deviceNode.Children.Count > 0}");
            
            // Store for drag operation - IMPORTANT: Store the TreeViewItem that was actually clicked
            _draggingItem = clickedItem;
            _pointerPressedPosition = e.GetPosition(this);
            _isDragging = false;
            
            System.Diagnostics.Debug.WriteLine($"Stored _draggingItem with DataContext: {(_draggingItem.DataContext as DeviceNode)?.Name ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"Pointer pressed at: {_pointerPressedPosition}, captured: {e.Pointer.Captured != null}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"ERROR: Could not find TreeViewItem with DeviceNode DataContext");
            System.Diagnostics.Debug.WriteLine($"Sender type: {sender?.GetType().Name ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"Hit test result: {hitTestResult?.GetType().Name ?? "null"}");
            if (sender is TreeViewItem item)
            {
                System.Diagnostics.Debug.WriteLine($"Sender DataContext type: {item.DataContext?.GetType().Name ?? "null"}");
            }
        }
    }
    
    private void TreeViewItem_PointerMoved(object? sender, PointerEventArgs e)
    {
        // Check if we have a dragging item and button is still pressed
        if (_draggingItem == null || !e.GetCurrentPoint(_draggingItem).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var currentPosition = e.GetPosition(this);
        var deltaX = Math.Abs(currentPosition.X - _pointerPressedPosition.X);
        var deltaY = Math.Abs(currentPosition.Y - _pointerPressedPosition.Y);

        System.Diagnostics.Debug.WriteLine($"TreeViewItem_PointerMoved: deltaX={deltaX:F2}, deltaY={deltaY:F2}, threshold={DragThreshold}, isDragging={_isDragging}");

        // Start drag if moved beyond threshold
        if (!_isDragging && (deltaX > DragThreshold || deltaY > DragThreshold))
        {
            _isDragging = true;
            System.Diagnostics.Debug.WriteLine("========== STARTING DRAG OPERATION ==========");

            // Visual feedback: make dragging item more visible
            if (_draggingItem != null)
            {
                // Reduce opacity to show dragging state
                _draggingItem.Opacity = 0.5;
                // Change cursor to show dragging
                _draggingItem.Cursor = new Cursor(StandardCursorType.DragMove);
            }

            // Get the DataContext from the TreeViewItem (which is the DeviceNode)
            // IMPORTANT: _draggingItem.DataContext should be the exact node being dragged (child or parent)
            // Double-check: Get DataContext again to ensure we have the correct node
            if (_draggingItem == null)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: _draggingItem is null");
                return;
            }
            
            // Get DataContext directly from the stored TreeViewItem
            var dataContext = _draggingItem.DataContext;
            System.Diagnostics.Debug.WriteLine($"========== DRAG START ==========");
            System.Diagnostics.Debug.WriteLine($"_draggingItem.DataContext type: {dataContext?.GetType().Name ?? "null"}");
            
            if (dataContext is DeviceNode deviceNode)
            {
                // Check if it's a parent node (has children) or child node
                var isParent = deviceNode.Children != null && deviceNode.Children.Count > 0;
                
                System.Diagnostics.Debug.WriteLine($"Dragging node: {deviceNode.Name}");
                System.Diagnostics.Debug.WriteLine($"IsParent: {isParent}");
                System.Diagnostics.Debug.WriteLine($"Has Children: {deviceNode.Children != null && deviceNode.Children.Count > 0}");
                if (deviceNode.Children != null && deviceNode.Children.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Children count: {deviceNode.Children.Count}");
                    foreach (var child in deviceNode.Children)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - Child: {child.Name}");
                    }
                }
                
                // Create drag data - use the EXACT node being dragged
                // This deviceNode is the node that was actually clicked/dragged (child or parent)
                var dragData = new DataObject();
                dragData.Set("DeviceNode", deviceNode);  // This is the node being dragged (child or parent)
                dragData.Set("IsParent", isParent);
                dragData.Set("DeviceName", deviceNode.Name);  // Use the name of the dragged node
                
                System.Diagnostics.Debug.WriteLine($"✅ Drag data set: DeviceNode={deviceNode.Name}, DeviceName={deviceNode.Name}, IsParent={isParent}");
                System.Diagnostics.Debug.WriteLine($"Starting DoDragDrop...");
                
                // Start drag operation
                var result = DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move);
                
                System.Diagnostics.Debug.WriteLine($"DragDrop.DoDragDrop completed with result: {result}");
                
                // Restore visual state after drag
                if (_draggingItem != null)
                {
                    _draggingItem.Opacity = 1.0;
                    _draggingItem.Cursor = new Cursor(StandardCursorType.Hand);
                }
                
                System.Diagnostics.Debug.WriteLine($"========== DRAG END ==========");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR: TreeViewItem.DataContext is not DeviceNode, it's: {dataContext?.GetType().Name ?? "null"}");
            }
        }
    }
    
    private void TreeViewItem_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("TreeViewItem_PointerReleased called");
        
        // Release pointer capture
        if (e.Pointer.Captured != null)
        {
            e.Pointer.Capture(null);
        }
        
        // Restore visual state if drag was cancelled
        if (_draggingItem != null && _isDragging)
        {
            _draggingItem.Opacity = 1.0;
            _draggingItem.Cursor = new Cursor(StandardCursorType.Hand);
        }
        
        _isDragging = false;
        _draggingItem = null;
        if (_deviceTreeView != null)
        {
            _deviceTreeView.Tag = null;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_deviceTreeView != null)
        {
            _deviceTreeView.ContainerPrepared -= TreeView_ContainerPrepared;
            _deviceTreeView = null;
        }
        base.OnDetachedFromVisualTree(e);
    }
}

