# LivePageView - Phân Tích Logic

## 📋 Tổng Quan

`LivePageView` là view chính quản lý:
- **Sidebar**: Hiển thị danh sách devices (DeviceExpanderView, EventExpanderView, NotificationExpanderView)
- **Tab Area**: Khu vực drop zone và hiển thị các node buttons (tabs)
- **Content Area**: Hiển thị content tương ứng với node được chọn

---

## 🏗️ Cấu Trúc UI

```
┌─────────────────────────────────────────────────────────────┐
│                        LivePageView                          │
├──────────────┬──────────────────────────────────────────────┤
│              │  ┌────────────────────────────────────────┐  │
│   SIDEBAR    │  │  Top Bar (Drop Zone + Buttons)        │  │
│              │  │  ┌────────────────────────────────┐  │  │
│  ┌────────┐  │  │  │ TabContainerPanel              │  │  │
│  │Device  │  │  │  │ [Node1][X] [Node2][X] ...     │  │  │
│  │Expander│  │  │  └────────────────────────────────┘  │  │
│  └────────┘  │  │  ┌────────────────────────────────┐  │  │
│  ┌────────┐  │  │  │ DropHintText (when no tabs)    │  │  │
│  │Event   │  │  │  └────────────────────────────────┘  │  │
│  │Expander│  │  └────────────────────────────────────────┘  │
│  └────────┘  │  ┌────────────────────────────────────────┐  │
│  ┌────────┐  │  │  Content Area (TabContentControl)     │  │
│  │Notif   │  │  │  ┌──────────────────────────────────┐ │  │
│  │Expander│  │  │  │ CameraListView / CameraDetailView│ │  │
│  └────────┘  │  │  └──────────────────────────────────┘ │  │
│              │  └────────────────────────────────────────┘  │
└──────────────┴──────────────────────────────────────────────┘
```

---

## 🔄 Flow: Drag & Drop

```
┌─────────────────────────────────────────────────────────────┐
│ 1. USER DRAGS NODE FROM SIDEBAR                            │
└─────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────┐
│ DeviceExpanderView.TreeViewItem_PointerMoved               │
│ - Detect drag threshold (>5px)                              │
│ - Get DeviceNode from TreeViewItem.DataContext              │
│ - Create DataObject with:                                   │
│   • DeviceNode (exact node dragged)                         │
│   • IsParent (has children?)                                │
│   • DeviceName (node.Name)                                  │
└─────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────┐
│ DragDrop.DoDragDrop()                                      │
└─────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. DRAG OVER DROP ZONE                                      │
└─────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────┐
│ LivePageView.TabControl_DragOver                           │
│ - Check if data contains "DeviceNode"                       │
│ - Set DragDropEffects.Move                                   │
│ - ShowDropZoneFeedback() (visual feedback)                  │
└─────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. USER DROPS NODE                                          │
└─────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────┐
│ LivePageView.TabControl_Drop                                │
│                                                             │
│ 1. Get DeviceNode from drag data                            │
│ 2. Extract: deviceName = deviceNode.Name                    │
│             isParent = deviceNode.Children?.Count > 0       │
│                                                             │
│ 3. Check duplicate:                                         │
│    - Loop through _nodeButtons                              │
│    - If (DeviceName == deviceName && IsParent == isParent)   │
│      → Skip (duplicate)                                     │
│                                                             │
│ 4. Create tab info:                                         │
│    viewModel.CreateTabInfoFromDeviceNode(deviceName,        │
│                                          isParent)          │
│                                                             │
│ 5. CreateNodeButtonWithClose(deviceName, isParent)         │
│                                                             │
│ 6. Show TabContainerPanel                                    │
│                                                             │
│ 7. SelectNodeButton(newButton)                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 Quản Lý Node Buttons

### Data Structure

```csharp
// Dictionary mapping: Button → (Container, CloseButton, DeviceName, IsParent)
Dictionary<Button, (StackPanel Container, 
                   Button CloseButton, 
                   string DeviceName, 
                   bool IsParent)> _nodeButtons

// Selected node button
Button? _selectedNodeButton
```

### Create Node Button

```
┌─────────────────────────────────────────────────────────────┐
│ CreateNodeButtonWithClose(deviceName, isParent)            │
│                                                             │
│ 1. Find TabContainerPanel                                   │
│                                                             │
│ 2. Create StackPanel (Container)                            │
│    └─ NodeButton (Button with deviceName)                   │
│    └─ CloseButton (Button with "×")                          │
│                                                             │
│ 3. Add Container to TabContainerPanel                        │
│                                                             │
│ 4. Store in _nodeButtons dictionary:                        │
│    _nodeButtons[nodeButton] = (container, closeButton,      │
│                                deviceName, isParent)        │
└─────────────────────────────────────────────────────────────┘
```

### Select Node Button

```
┌─────────────────────────────────────────────────────────────┐
│ SelectNodeButton(nodeButton)                                │
│                                                             │
│ 1. Deselect all buttons:                                    │
│    - Remove "NodeButtonSelectedStyle" from all              │
│    - Remove "CloseButtonSelectedStyle" from all             │
│                                                             │
│ 2. Select clicked button:                                   │
│    - Add "NodeButtonSelectedStyle" to nodeButton            │
│    - Add "CloseButtonSelectedStyle" to closeButton          │
│                                                             │
│ 3. Update _selectedNodeButton                                │
│                                                             │
│ 4. Call OnTabChanged() to update content                    │
└─────────────────────────────────────────────────────────────┘
```

### Remove Node Button

```
┌─────────────────────────────────────────────────────────────┐
│ RemoveNodeButton(nodeButton)                                │
│                                                             │
│ 1. Get info from _nodeButtons                               │
│                                                             │
│ 2. Check if was selected                                    │
│                                                             │
│ 3. Remove Container from TabContainerPanel                  │
│                                                             │
│ 4. Remove from _nodeButtons dictionary                      │
│                                                             │
│ 5. If no buttons left:                                      │
│    - Hide TabContainerPanel                                 │
│    - Show DropHintText                                      │
│    - Clear TabContentControl.Content                        │
│                                                             │
│ 6. Else if was selected:                                    │
│    - Select first button                                    │
│    - OR clear content if no buttons                         │
└─────────────────────────────────────────────────────────────┘
```

---

## 📦 Content Management

### OnTabChanged Flow

```
┌─────────────────────────────────────────────────────────────┐
│ OnTabChanged()                                              │
│                                                             │
│ 1. Check _selectedNodeButton                                │
│    - If null → Clear content, return                        │
│                                                             │
│ 2. Get node info from _nodeButtons:                        │
│    - DeviceName                                             │
│    - IsParent                                               │
│                                                             │
│ 3. Clear existing content:                                  │
│    _tabContentControl.Content = null                       │
│                                                             │
│ 4. Create new content via ViewModel:                        │
│    viewModel.CreateTabContent(deviceName, isParent)        │
│                                                             │
│    ┌─────────────────────────────────────┐                 │
│    │ if (isParent == true)               │                 │
│    │   → Create CameraListView           │                 │
│    │ else                                │                 │
│    │   → Create CameraDetailView         │                 │
│    └─────────────────────────────────────┘                 │
│                                                             │
│ 5. Set content:                                             │
│    _tabContentControl.Content = tabContent                  │
└─────────────────────────────────────────────────────────────┘
```

### ViewModel Methods

```
┌─────────────────────────────────────────────────────────────┐
│ LivePageViewModel.CreateTabInfoFromDeviceNode()            │
│                                                             │
│ Input:  deviceName, isParent                               │
│ Output: (TabHeader, DeviceName, IsParent)                  │
│                                                             │
│ Expression body:                                            │
│   => (deviceName, deviceName, isParent)                    │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ LivePageViewModel.CreateTabContent()                        │
│                                                             │
│ Input:  deviceName, isParent                               │
│ Output: Control (CameraListView or CameraDetailView)       │
│                                                             │
│ if (isParent)                                               │
│   → new CameraListView {                                    │
│        DataContext = new CameraListViewModel(deviceName)   │
│      }                                                      │
│                                                             │
│ // Early return (no else needed)                             │
│ → new CameraDetailView {                                    │
│      DataContext = new CameraDetailViewModel(deviceName)   │
│    }                                                        │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎛️ Sidebar Management

### ViewModel Properties

```
┌─────────────────────────────────────────────────────────────┐
│ LivePageViewModel Properties                                │
│                                                             │
│ [ObservableProperty]                                       │
│ private bool _isSidebarOpen = true                          │
│                                                             │
│ public bool IsSidebarVisible => IsSidebarOpen              │
│ public bool IsDeviceButtonVisible => !IsSidebarOpen        │
└─────────────────────────────────────────────────────────────┘
```

### Commands

```
┌─────────────────────────────────────────────────────────────┐
│ OpenSidebarCommand                                          │
│ Expression body: => IsSidebarOpen = true                   │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ CloseSidebarCommand                                         │
│ Expression body: => IsSidebarOpen = false                   │
└─────────────────────────────────────────────────────────────┘
```

### Sidebar State Flow

```
┌─────────────────────────────────────────────────────────────┐
│ Initial State:                                              │
│   IsSidebarOpen = true                                      │
│   IsSidebarVisible = true                                   │
│   IsDeviceButtonVisible = false                             │
└─────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────┐
│ User clicks "Close" button:                                 │
│   CloseSidebarCommand → IsSidebarOpen = false               │
│   → IsSidebarVisible = false                                │
│   → IsDeviceButtonVisible = true                            │
└─────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────┐
│ User clicks "Device" button:                                │
│   OpenSidebarCommand → IsSidebarOpen = true                │
│   → IsSidebarVisible = true                                 │
│   → IsDeviceButtonVisible = false                           │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎨 Visual Feedback

### Drop Zone Feedback

```
┌─────────────────────────────────────────────────────────────┐
│ ShowDropZoneFeedback(data)                                 │
│                                                             │
│ 1. Add "DropZoneBorderDragOverStyle" to DropZoneBorder     │
│                                                             │
│ 2. Update DropHintText:                                     │
│    - Get deviceName and isParent from data                  │
│    - Set text: "📥 Drop to create: {viewType}              │
│                for \"{deviceName}\""                        │
│    - Add "DropHintTextDragOverStyle"                        │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ HideDropZoneFeedback()                                     │
│                                                             │
│ 1. Remove "DropZoneBorderDragOverStyle"                     │
│                                                             │
│ 2. Reset DropHintText:                                       │
│    - Set text: "📥 Drop device here to create tab"         │
│    - Remove "DropHintTextDragOverStyle"                     │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔑 Key Components

### Fields

| Field | Type | Purpose |
|-------|------|---------|
| `_tabContentControl` | `ContentControl?` | Hiển thị content (CameraListView/DetailView) |
| `_tabContainerPanel` | `StackPanel?` | Chứa các node buttons |
| `_nodeButtons` | `Dictionary<Button, ...>` | Map node button → info |
| `_selectedNodeButton` | `Button?` | Node button đang được chọn |
| `_dropZoneBorder` | `Border?` | Drop zone border |
| `_dropHintText` | `TextBlock?` | Hint text khi không có tabs |

### Methods

| Method | Purpose |
|--------|---------|
| `CreateNodeButtonWithClose()` | Tạo node button + close button |
| `SelectNodeButton()` | Chọn node button và update content |
| `RemoveNodeButton()` | Xóa node button |
| `OnTabChanged()` | Update content khi tab thay đổi |
| `TabControl_DragOver()` | Handle drag over event |
| `TabControl_Drop()` | Handle drop event |
| `ShowDropZoneFeedback()` | Hiển thị visual feedback khi drag |
| `HideDropZoneFeedback()` | Ẩn visual feedback |

---

## 🔄 State Management

### Tab Container Panel Visibility

```
┌─────────────────────────────────────────────────────────────┐
│ Initial State:                                              │
│   TabContainerPanel.IsVisible = false                       │
│   DropHintText.IsVisible = true                             │
└─────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────┐
│ After First Tab Created:                                    │
│   TabContainerPanel.IsVisible = true                         │
│   DropHintText.IsVisible = false                            │
└─────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────┐
│ After All Tabs Removed:                                     │
│   TabContainerPanel.IsVisible = false                       │
│   DropHintText.IsVisible = true                             │
└─────────────────────────────────────────────────────────────┘
```

### Content State

```
┌─────────────────────────────────────────────────────────────┐
│ No Tab Selected:                                             │
│   _selectedNodeButton = null                                 │
│   _tabContentControl.Content = null                         │
└─────────────────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────────┐
│ Tab Selected:                                                │
│   _selectedNodeButton = nodeButton                           │
│   _tabContentControl.Content = CameraListView/DetailView     │
└─────────────────────────────────────────────────────────────┘
```

---

## 🐛 Debug Flow

### Drag & Drop Debug Logs

```
1. DeviceExpanderView:
   - "========== POINTER PRESSED =========="
   - "TreeViewItem_PointerPressed: {deviceNode.Name}"
   - "========== DRAG START =========="
   - "Dragging node: {deviceNode.Name}"
   - "IsParent: {isParent}"

2. LivePageView:
   - "TabControl_DragOver called"
   - "========== TabControl_Drop CALLED =========="
   - "========== DROP START =========="
   - "DeviceNode from data: {deviceNode.Name}"
   - "Using deviceName: {deviceName}"
   - "Created tab info: Header={tabHeader}, ..."
   - "✅ SUCCESS: Dropped device: {deviceName}, ..."
```

---

## 📝 Notes

1. **Duplicate Prevention**: Kiểm tra duplicate dựa trên `DeviceName` + `IsParent`
2. **Content Creation**: Mỗi lần select tab, tạo mới control instance để tránh "already has visual parent" error
3. **Node Button Selection**: Chỉ một node button được chọn tại một thời điểm
4. **Visual Feedback**: Drop zone có visual feedback khi drag over
5. **Sidebar Management**: 
   - Sidebar mở mặc định (`IsSidebarOpen = true`)
   - Toggle giữa sidebar và device button dựa trên `IsSidebarOpen`
   - Sử dụng expression body methods cho clean code
6. **Code Cleanup**: 
   - Đã loại bỏ các properties/methods không sử dụng (PrintList, TypeBook, etc.)
   - ViewModel chỉ giữ lại logic cần thiết cho LivePageView
   - Sử dụng expression body và early return pattern

