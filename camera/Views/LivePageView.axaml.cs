using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using camera.Data;
using camera.ViewModels;
using camera.Controls;
using camera.Factories;

namespace camera.Views;

/// <summary>
/// Helper class to store tab information without storing control instances
/// </summary>
internal class TabInfo
{
    public string DeviceName { get; set; } = string.Empty;
    public bool IsParent { get; set; }
}

public partial class LivePageView : UserControl
{
    private ContentControl? _tabContentControl;
    private StackPanel? _tabContainerPanel;
    private Dictionary<Button, (StackPanel Container, Button CloseButton, string DeviceName, bool IsParent)> _nodeButtons = new();
    private Button? _selectedNodeButton;

    public LivePageView()
    {
        InitializeComponent();
    }

    private void NodeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button nodeButton && _nodeButtons.ContainsKey(nodeButton))
        {
            SelectNodeButton(nodeButton);
        }
    }
    
    private void SelectNodeButton(Button nodeButton)
    {
        // Deselect all buttons
        foreach (var (btn, info) in _nodeButtons)
        {
            // Update node button style to unselected - remove selected class
            btn.Classes.Remove("NodeButtonSelectedStyle");
            
            // Update close button style to match unselected state - remove selected class
            info.CloseButton.Classes.Remove("CloseButtonSelectedStyle");
        }
        
        // Select the clicked button - add selected class
        nodeButton.Classes.Add("NodeButtonSelectedStyle");
        
        // Update close button style to match selected state
        if (_nodeButtons.TryGetValue(nodeButton, out var selectedInfo))
        {
            selectedInfo.CloseButton.Classes.Add("CloseButtonSelectedStyle");
        }
        
        _selectedNodeButton = nodeButton;
        
        // Update content
        OnTabChanged();
    }
    
    private void CreateNodeButtonWithClose(string deviceName, bool isParent)
    {
        // Find TabContainerPanel if not cached
        if (_tabContainerPanel == null)
        {
            _tabContainerPanel = this.FindControl<StackPanel>("TabContainerPanel");
        }
        
        if (_tabContainerPanel == null)
        {
            System.Diagnostics.Debug.WriteLine("❌ TabContainerPanel not found");
            return;
        }
        
        // Create container StackPanel cho cặp (Button Node + Button X)
        var container = new StackPanel
        {
            Classes = { "NodeContainerStyle" }
        };
        
        // Create node button (tên node) - sử dụng style class
        var nodeButton = new Button
        {
            Content = deviceName,
            Classes = { "NodeButtonStyle" }
        };
        
        // Attach click handler for selection
        nodeButton.Click += NodeButton_Click;
        
        // Create close button - sử dụng style class
        var closeButton = new Button
        {
            Classes = { "CloseButtonStyle" }
        };
        
        // Attach click handler for close
        closeButton.Click += (sender, e) =>
        {
            RemoveNodeButton(nodeButton);
        };
        
        // Add buttons to container
        container.Children.Add(nodeButton);
        container.Children.Add(closeButton);
        
        // Add container to TabContainerPanel
        _tabContainerPanel.Children.Add(container);
        
        // Store mapping
        _nodeButtons[nodeButton] = (container, closeButton, deviceName, isParent);
        
        System.Diagnostics.Debug.WriteLine($"✅ Node button with close button created: {deviceName}");
    }
    
    private void RemoveNodeButton(Button nodeButton)
    {
        if (!_nodeButtons.TryGetValue(nodeButton, out var info))
        {
            System.Diagnostics.Debug.WriteLine($"❌ Node button not found in dictionary");
            return;
        }
        
        var wasSelected = _selectedNodeButton == nodeButton;
        
        // Remove container from panel
        if (_tabContainerPanel != null)
        {
            _tabContainerPanel.Children.Remove(info.Container);
        }
        
        // Remove from dictionary
        _nodeButtons.Remove(nodeButton);
        
        System.Diagnostics.Debug.WriteLine($"✅ Node button removed: {info.DeviceName}");
        
        // If no buttons left, hide panel and show drop hint
        if (_tabContainerPanel != null && _tabContainerPanel.Children.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("No node buttons left, hiding TabContainerPanel");
            _tabContainerPanel.IsVisible = false;
            if (_dropHintText != null)
            {
                _dropHintText.IsVisible = true;
            }
            // Clear content
            if (_tabContentControl != null)
            {
                _tabContentControl.Content = null;
            }
            _selectedNodeButton = null;
        }
        else
        {
            // Select another button if current was removed
            if (wasSelected && _tabContainerPanel != null && _tabContainerPanel.Children.Count > 0)
            {
                // Select the first button
                var firstContainer = _tabContainerPanel.Children[0] as StackPanel;
                if (firstContainer != null && firstContainer.Children.Count > 0 && firstContainer.Children[0] is Button firstButton)
                {
                    SelectNodeButton(firstButton);
                }
            }
            else
            {
                // Clear content
                if (_tabContentControl != null)
                {
                    _tabContentControl.Content = null;
                }
            }
        }
    }


    private void OnTabChanged()
    {
        try
        {
            if (_selectedNodeButton == null)
            {
                System.Diagnostics.Debug.WriteLine("No node button selected");
                // Clear content when no button is selected
                if (_tabContentControl != null)
                {
                    _tabContentControl.Content = null;
                }
                return;
            }

            // Find the ContentControl in the content area (cache it)
            if (_tabContentControl == null)
            {
                _tabContentControl = this.FindControl<ContentControl>("TabContentControl");
                if (_tabContentControl == null)
                {
                    System.Diagnostics.Debug.WriteLine("TabContentControl not found");
                    return;
                }
            }

            // Get node info from dictionary
            if (!_nodeButtons.TryGetValue(_selectedNodeButton, out var nodeInfo))
            {
                System.Diagnostics.Debug.WriteLine("Selected node button not found in dictionary");
                if (_tabContentControl != null)
                {
                    _tabContentControl.Content = null;
                }
                return;
            }
            
            // Clear any existing content first to prevent conflicts
            if (_tabContentControl.Content != null)
            {
                _tabContentControl.Content = null;
            }
            
            // Create new control instance from stored info
            var viewModel = DataContext as LivePageViewModel;
            if (viewModel != null)
            {
                // Create fresh instance based on IsParent flag
                // IsParent = true → CameraListView (node có children)
                // IsParent = false → CameraDetailView (node không có children)
                var tabContent = viewModel.CreateTabContent(nodeInfo.DeviceName, nodeInfo.IsParent);
                
                if (tabContent != null)
                {
                    _tabContentControl.Content = tabContent;
                    System.Diagnostics.Debug.WriteLine($"✅ Content updated for node: {nodeInfo.DeviceName}, IsParent: {nodeInfo.IsParent}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Failed to create content for node: {nodeInfo.DeviceName}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ ViewModel is null");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ERROR in OnTabChanged: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    // ========== DRAG & DROP HANDLERS ==========

    private Border? _dropZoneBorder;
    private TextBlock? _dropHintText;

    private void TabControl_DragOver(object? sender, DragEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("TabControl_DragOver called");
        
        // Check if drag data contains DeviceNode
        var data = e.Data;
        System.Diagnostics.Debug.WriteLine($"DragOver - Data is null: {data == null}");
        
        if (data != null)
        {
            var hasDeviceNode = data.Contains("DeviceNode");
            System.Diagnostics.Debug.WriteLine($"DragOver - Contains DeviceNode: {hasDeviceNode}");
            
            if (hasDeviceNode)
            {
                e.DragEffects = DragDropEffects.Move;
                
                // Show visual feedback
                ShowDropZoneFeedback(data);
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
                HideDropZoneFeedback();
            }
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
            HideDropZoneFeedback();
        }
    }

    private void TabControl_DragLeave(object? sender, DragEventArgs e)
    {
        HideDropZoneFeedback();
    }

    private void TabControl_Drop(object? sender, DragEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("========== TabControl_Drop CALLED ==========");
        
        try
        {
            // Hide feedback first
            HideDropZoneFeedback();

            // Get device info from drag data using Data property
            var data = e.Data;
            if (data == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: e.Data is null");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Data contains DeviceNode: {data.Contains("DeviceNode")}");
            System.Diagnostics.Debug.WriteLine($"Data contains IsParent: {data.Contains("IsParent")}");
            System.Diagnostics.Debug.WriteLine($"Data contains DeviceName: {data.Contains("DeviceName")}");

            var deviceNode = data.Get("DeviceNode") as DeviceNode;
            var isParent = data.Get("IsParent") as bool? ?? false;
            var deviceNameFromData = data.Get("DeviceName") as string;

            System.Diagnostics.Debug.WriteLine($"========== DROP START ==========");
            System.Diagnostics.Debug.WriteLine($"DeviceNode from data: {deviceNode?.Name ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"DeviceName from data: {deviceNameFromData ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"IsParent from data: {isParent}");

            if (deviceNode == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: deviceNode is null");
                return;
            }

            // Use deviceNode.Name directly to ensure we use the correct node (child or parent)
            // deviceNode is the EXACT node that was dragged (could be child or parent)
            var deviceName = deviceNode.Name;
            
            System.Diagnostics.Debug.WriteLine($"Using deviceName: {deviceName} (from deviceNode.Name)");
            System.Diagnostics.Debug.WriteLine($"DeviceNode details: Name={deviceNode.Name}, HasChildren={deviceNode.Children != null && deviceNode.Children.Count > 0}");

            // Get ViewModel to create tab info
            var viewModel = DataContext as LivePageViewModel;
            if (viewModel == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: ViewModel is null");
                return;
            }

            System.Diagnostics.Debug.WriteLine("Creating node button from device node...");
            
            // Create tab info - use deviceNode.Name directly to ensure correct node
            var (tabHeader, tabDeviceName, tabIsParent) = viewModel.CreateTabInfoFromDeviceNode(deviceName, isParent);

            System.Diagnostics.Debug.WriteLine($"Created tab info: Header={tabHeader}, DeviceName={tabDeviceName}, IsParent={tabIsParent}");

            // Check for duplicate - same DeviceName and IsParent
            bool isDuplicate = false;
            
            foreach (var (_, info) in _nodeButtons)
            {
                if (info.DeviceName == tabDeviceName && info.IsParent == tabIsParent)
                {
                    isDuplicate = true;
                    System.Diagnostics.Debug.WriteLine($"Duplicate node button found: {tabHeader} (DeviceName={tabDeviceName}, IsParent={tabIsParent})");
                    break;
                }
            }
            
            if (isDuplicate)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Node button already exists, skipping: {tabHeader}");
                return; // Don't add duplicate
            }

            // Create node button with close button (adds to TabContainerPanel)
            CreateNodeButtonWithClose(tabDeviceName, tabIsParent);
            
            // Show TabContainerPanel when button is added
            if (_tabContainerPanel != null)
            {
                _tabContainerPanel.IsVisible = true;
                System.Diagnostics.Debug.WriteLine($"✅ TabContainerPanel is now visible");
            }
            
            if (_dropHintText != null)
            {
                _dropHintText.IsVisible = false;
            }

            // Select the new button (find it in the panel)
            if (_tabContainerPanel != null && _tabContainerPanel.Children.Count > 0)
            {
                var lastContainer = _tabContainerPanel.Children[_tabContainerPanel.Children.Count - 1] as StackPanel;
                if (lastContainer != null && lastContainer.Children.Count > 0 && lastContainer.Children[0] is Button newButton)
                {
                    System.Diagnostics.Debug.WriteLine($"Selecting new node button: {tabDeviceName}");
                    SelectNodeButton(newButton);
                }
            }
        
            System.Diagnostics.Debug.WriteLine($"✅ SUCCESS: Dropped device: {deviceName}, IsParent: {isParent}, Created tab: {tabHeader}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ERROR in TabControl_Drop: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }

    private void ShowDropZoneFeedback(IDataObject data)
    {
        // Lazy initialization
        if (_dropZoneBorder == null)
        {
            _dropZoneBorder = this.FindControl<Border>("DropZoneBorder");
        }
        if (_dropHintText == null)
        {
            _dropHintText = this.FindControl<TextBlock>("DropHintText");
        }

        if (_dropZoneBorder != null)
        {
            // Change to drag over style
            _dropZoneBorder.Classes.Add("DropZoneBorderDragOverStyle");
        }

        // Update hint text
        if (_dropHintText != null && data != null)
        {
            var deviceName = data.Get("DeviceName") as string ?? "Device";
            var isParent = data.Get("IsParent") as bool? ?? false;
            var viewType = isParent ? "Camera List" : "Camera Detail";
            _dropHintText.Text = $"📥 Drop to create: {viewType}\nfor \"{deviceName}\"";
            _dropHintText.Classes.Add("DropHintTextDragOverStyle");
        }
    }

    private void HideDropZoneFeedback()
    {
        if (_dropZoneBorder != null)
        {
            // Remove drag over style to restore default
            _dropZoneBorder.Classes.Remove("DropZoneBorderDragOverStyle");
        }

        if (_dropHintText != null)
        {
            _dropHintText.Text = "📥 Drop device here to create tab";
            _dropHintText.Classes.Remove("DropHintTextDragOverStyle");
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // Ensure controls are loaded before accessing them
        Dispatcher.UIThread.Post(() => OnTabChanged(), DispatcherPriority.Loaded);
    }
}
