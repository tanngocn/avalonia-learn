# Hướng Dẫn Xử Lý Tabs Động - Không Cần If-Else

Guide này giải thích cách xử lý tabs động mà không cần dùng if-else liên tục.

## 🎯 Vấn Đề

Khi có nhiều tabs, việc dùng if-else như này không scalable:

```csharp
if (selectedIndex == 0)
    tabContent = new LivePrintView();
else if (selectedIndex == 1)
    tabContent = new LiveLogView();
else if (selectedIndex == 2)
    tabContent = new SettingsView();
// ... và cứ thế tiếp tục
```

## ✅ Giải Pháp: Factory Pattern với Dictionary

Sử dụng `TabContentFactory` để map tab header với View type.

---

## 📝 Cách Sử Dụng

### 1. Tab Đã Được Đăng Ký Sẵn

Các tab đã được đăng ký trong `TabContentFactory.cs`:

```csharp
private static readonly Dictionary<string, TabInfo> TabMapping = new()
{
    { "Print", new TabInfo(typeof(LivePrintView), LiveTabNames.Print) },
    { "Log", new TabInfo(typeof(LiveLogView), LiveTabNames.Log) },
};
```

### 2. Sử Dụng trong View

```csharp
// Thay vì if-else, chỉ cần gọi Factory:
var tabContent = TabContentFactory.CreateTabContent(LiveTabControls, selectedIndex);
var liveTabName = TabContentFactory.GetTabName(LiveTabControls, selectedIndex);
```

### 3. Thêm Tab Mới

#### Cách 1: Thêm vào Dictionary trong Factory (Static)

Mở `TabContentFactory.cs` và thêm vào `TabMapping`:

```csharp
private static readonly Dictionary<string, TabInfo> TabMapping = new()
{
    { "Print", new TabInfo(typeof(LivePrintView), LiveTabNames.Print) },
    { "Log", new TabInfo(typeof(LiveLogView), LiveTabNames.Log) },
    { "Settings", new TabInfo(typeof(SettingsView), LiveTabNames.Settings) }, // ← Thêm mới
    { "Reports", new TabInfo(typeof(ReportsView), LiveTabNames.Reports) },   // ← Thêm mới
};
```

**Lưu ý:** Cần thêm enum value vào `LiveTabNames`:

```csharp
public enum LiveTabNames
{
    Unknown,
    Print,
    Log,
    Settings,  // ← Thêm mới
    Reports    // ← Thêm mới
}
```

#### Cách 2: Đăng Ký Động (Runtime)

Có thể đăng ký tab mới trong runtime:

```csharp
// Trong ViewModel hoặc Service
TabContentFactory.RegisterTab(
    header: "Settings",
    viewType: typeof(SettingsView),
    tabName: LiveTabNames.Settings
);
```

---

## 🔧 Ví Dụ Hoàn Chỉnh

### Thêm Tab "Settings"

**Bước 1:** Tạo View `SettingsView.axaml` và `SettingsView.axaml.cs`

**Bước 2:** Thêm enum value:

```csharp
// Data/LiveTabNames.cs
public enum LiveTabNames
{
    Unknown,
    Print,
    Log,
    Settings  // ← Thêm mới
}
```

**Bước 3:** Thêm vào Factory:

```csharp
// Factories/TabContentFactory.cs
private static readonly Dictionary<string, TabInfo> TabMapping = new()
{
    { "Print", new TabInfo(typeof(LivePrintView), LiveTabNames.Print) },
    { "Log", new TabInfo(typeof(LiveLogView), LiveTabNames.Log) },
    { "Settings", new TabInfo(typeof(SettingsView), LiveTabNames.Settings) }, // ← Thêm
};
```

**Bước 4:** Thêm TabItem vào XAML:

```xml
<controls:TabControlCustom ...>
    <controls:TabItemCustom Header="Print" IsSelected="True" />
    <controls:TabItemCustom Header="Log" />
    <controls:TabItemCustom Header="Settings" /> <!-- ← Thêm mới -->
</controls:TabControlCustom>
```

**Xong!** Không cần sửa code-behind, Factory sẽ tự động xử lý.

---

## 🎨 Advanced: Tabs Hoàn Toàn Động

Nếu muốn tabs hoàn toàn động (load từ config, database, API), có thể:

### 1. Tạo TabInfo Model

```csharp
public class TabInfoModel
{
    public string Header { get; set; }
    public Type ViewType { get; set; }
    public LiveTabNames TabName { get; set; }
}
```

### 2. Load từ ViewModel

```csharp
public partial class LivePageViewModel : PageViewModel
{
    [ObservableProperty]
    private ObservableCollection<TabInfoModel> availableTabs = new();

    public LivePageViewModel()
    {
        // Load tabs từ service/config
        LoadTabs();
    }

    private void LoadTabs()
    {
        // Ví dụ: Load từ service
        var tabs = tabService.GetAvailableTabs();
        
        foreach (var tab in tabs)
        {
            // Đăng ký vào Factory
            TabContentFactory.RegisterTab(
                tab.Header,
                tab.ViewType,
                tab.TabName
            );
            
            AvailableTabs.Add(tab);
        }
    }
}
```

### 3. Bind Tabs trong XAML

```xml
<controls:TabControlCustom ItemsSource="{Binding AvailableTabs}">
    <controls:TabControlCustom.ItemTemplate>
        <DataTemplate>
            <controls:TabItemCustom Header="{Binding Header}" />
        </DataTemplate>
    </controls:TabControlCustom.ItemTemplate>
</controls:TabControlCustom>
```

---

## 📊 So Sánh

### ❌ Cách Cũ (If-Else)

```csharp
Control? tabContent = null;
if (selectedIndex == 0)
    tabContent = new LivePrintView();
else if (selectedIndex == 1)
    tabContent = new LiveLogView();
else if (selectedIndex == 2)
    tabContent = new SettingsView();
// ... phải thêm if-else mỗi khi có tab mới
```

**Vấn đề:**
- Phải sửa code mỗi khi thêm tab
- Khó maintain khi có nhiều tabs
- Dễ sai sót

### ✅ Cách Mới (Factory)

```csharp
var tabContent = TabContentFactory.CreateTabContent(LiveTabControls, selectedIndex);
var liveTabName = TabContentFactory.GetTabName(LiveTabControls, selectedIndex);
```

**Ưu điểm:**
- ✅ Không cần sửa code-behind khi thêm tab
- ✅ Dễ maintain và mở rộng
- ✅ Centralized mapping
- ✅ Có thể đăng ký động

---

## 🔍 API Reference

### `TabContentFactory.CreateTabContent(header)`
Tạo content từ header string.

```csharp
var content = TabContentFactory.CreateTabContent("Print");
```

### `TabContentFactory.CreateTabContent(tabControl, index)`
Tạo content từ TabControl và index.

```csharp
var content = TabContentFactory.CreateTabContent(LiveTabControls, 0);
```

### `TabContentFactory.GetTabName(header)`
Lấy LiveTabNames từ header.

```csharp
var tabName = TabContentFactory.GetTabName("Print"); // Returns LiveTabNames.Print
```

### `TabContentFactory.RegisterTab(header, viewType, tabName)`
Đăng ký tab mới động.

```csharp
TabContentFactory.RegisterTab("Settings", typeof(SettingsView), LiveTabNames.Settings);
```

### `TabContentFactory.HasTab(header)`
Kiểm tra tab có tồn tại không.

```csharp
if (TabContentFactory.HasTab("Print"))
{
    // Tab exists
}
```

---

## 💡 Best Practices

1. **Đặt tên header nhất quán**: Sử dụng cùng tên trong XAML và Factory
2. **Thêm enum value**: Luôn thêm vào `LiveTabNames` khi có tab mới
3. **Error handling**: Factory đã có try-catch, nhưng nên log lỗi
4. **Validation**: Kiểm tra view type có inherit từ Control không

---

## 🚀 Tóm Tắt

- ✅ Sử dụng Factory thay vì if-else
- ✅ Dễ thêm tab mới (chỉ cần thêm vào Dictionary)
- ✅ Có thể đăng ký động
- ✅ Code sạch và maintainable

Bây giờ bạn có thể thêm bao nhiêu tabs cũng được mà không cần sửa code-behind! 🎉

