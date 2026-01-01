# Ví Dụ Layout Avalonia - Button + TabControl + Content Area

Hướng dẫn tạo layout giống như trong hình: Button trái, TabControl giữa, Button phải, và vùng content lớn.

## 📋 Layout Structure

```
┌─────────────────────────────────────────┐
│ [Button] [Tab1] [Tab2]        [Button]  │
├─────────────────────────────────────────┤
│                                         │
│         Tab Content Area                │
│                                         │
│                                         │
└─────────────────────────────────────────┘
```

---

## 1. Tạo View với Layout Cơ Bản

Tạo file `Views/ExamplePageView.axaml`:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="using:camera.ViewModels"
             mc:Ignorable="d" d:DesignWidth="1200" d:DesignHeight="800"
             x:DataType="vm:ExamplePageViewModel"
             x:Class="camera.Views.ExamplePageView">
    
    <Design.DataContext>
        <vm:ExamplePageViewModel />
    </Design.DataContext>

    <Grid RowDefinitions="Auto, *">
        <!-- Top Bar: Button + Tabs + Button -->
        <Grid Grid.Row="0" ColumnDefinitions="Auto, *, Auto" 
              Background="#F5F5F5" 
              Padding="10">
            
            <!-- Left Button -->
            <Button Grid.Column="0" 
                    Content="Button" 
                    Command="{Binding LeftButtonCommand}"
                    Margin="0,0,10,0"
                    Padding="15,8" />
            
            <!-- TabControl in the middle -->
            <TabControl Grid.Column="1" 
                       SelectedIndex="{Binding SelectedTabIndex}"
                       HorizontalAlignment="Center">
                <TabItem Header="Tab1">
                    <TextBlock Text="Tab 1 Content" 
                              HorizontalAlignment="Center" 
                              VerticalAlignment="Center" />
                </TabItem>
                <TabItem Header="Tab2">
                    <TextBlock Text="Tab 2 Content" 
                              HorizontalAlignment="Center" 
                              VerticalAlignment="Center" />
                </TabItem>
            </TabControl>
            
            <!-- Right Button -->
            <Button Grid.Column="2" 
                    Content="Button" 
                    Command="{Binding RightButtonCommand}"
                    Margin="10,0,0,0"
                    Padding="15,8" />
        </Grid>

        <!-- Content Area -->
        <Border Grid.Row="1" 
                Background="White"
                BorderBrush="#E0E0E0"
                BorderThickness="1">
            <ContentControl Content="{Binding TabContent}" />
        </Border>
    </Grid>
</UserControl>
```

---

## 2. Tạo ViewModel

Tạo file `ViewModels/ExamplePageViewModel.cs`:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;

namespace camera.ViewModels;

public partial class ExamplePageViewModel : ViewModelBase
{
    [ObservableProperty]
    private int selectedTabIndex = 0;

    [ObservableProperty]
    private Control? tabContent;

    public ExamplePageViewModel()
    {
        // Load initial tab content
        UpdateTabContent();
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        UpdateTabContent();
    }

    private void UpdateTabContent()
    {
        TabContent = SelectedTabIndex switch
        {
            0 => new TextBlock 
            { 
                Text = "Tab 1 Content\n\nThis is the content area for Tab 1.",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                FontSize = 16
            },
            1 => new TextBlock 
            { 
                Text = "Tab 2 Content\n\nThis is the content area for Tab 2.",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                FontSize = 16
            },
            _ => new TextBlock { Text = "Unknown Tab" }
        };
    }

    [RelayCommand]
    private void LeftButton()
    {
        // Handle left button click
        Console.WriteLine("Left button clicked");
    }

    [RelayCommand]
    private void RightButton()
    {
        // Handle right button click
        Console.WriteLine("Right button clicked");
    }
}
```

---

## 3. Layout Nâng Cao với Custom Styling

### 3.1. View với Styling Đẹp Hơn

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:camera.ViewModels"
             x:Class="camera.Views.ExamplePageView"
             x:DataType="vm:ExamplePageViewModel">
    
    <UserControl.Styles>
        <Style Selector="TabItem">
            <Setter Property="Padding" Value="20,10" />
            <Setter Property="FontSize" Value="14" />
        </Style>
        
        <Style Selector="TabItem:selected">
            <Setter Property="Background" Value="#0078D4" />
            <Setter Property="Foreground" Value="White" />
        </Style>
        
        <Style Selector="Button">
            <Setter Property="Background" Value="#0078D4" />
            <Setter Property="Foreground" Value="White" />
            <Setter Property="BorderThickness" Value="0" />
            <Setter Property="CornerRadius" Value="4" />
        </Style>
        
        <Style Selector="Button:pointerover">
            <Setter Property="Background" Value="#106EBE" />
        </Style>
    </UserControl.Styles>

    <Grid RowDefinitions="Auto, *">
        <!-- Top Bar -->
        <Border Grid.Row="0" 
                Background="#F8F8F8" 
                BorderBrush="#E0E0E0"
                BorderThickness="0,0,0,1"
                Padding="15">
            <Grid ColumnDefinitions="Auto, *, Auto">
                
                <!-- Left Button -->
                <Button Grid.Column="0" 
                       Content="Button"
                       Command="{Binding LeftButtonCommand}"
                       Width="100" />
                
                <!-- TabControl -->
                <TabControl Grid.Column="1" 
                           SelectedIndex="{Binding SelectedTabIndex}"
                           HorizontalAlignment="Center"
                           Margin="20,0">
                    <TabItem Header="Tab1" />
                    <TabItem Header="Tab2" />
                </TabControl>
                
                <!-- Right Button -->
                <Button Grid.Column="2" 
                       Content="Button"
                       Command="{Binding RightButtonCommand}"
                       Width="100" />
            </Grid>
        </Border>

        <!-- Content Area -->
        <Border Grid.Row="1" 
                Background="White"
                Padding="20">
            <ContentControl Content="{Binding TabContent}" />
        </Border>
    </Grid>
</UserControl>
```

---

## 4. Layout với Custom TabControl (Như trong Project)

Nếu bạn muốn sử dụng `TabControlCustom` đã có:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:controls="using:camera.Controls"
             xmlns:vm="using:camera.ViewModels"
             x:Class="camera.Views.ExamplePageView"
             x:DataType="vm:ExamplePageViewModel">
    
    <Grid RowDefinitions="Auto, *">
        <!-- Top Bar -->
        <Grid Grid.Row="0" 
              ColumnDefinitions="Auto, *, Auto"
              Background="#F5F5F5"
              Padding="10">
            
            <!-- Left Button -->
            <Button Grid.Column="0" 
                   Content="Button"
                   Command="{Binding LeftButtonCommand}"
                   Margin="0,0,10,0" />
            
            <!-- Custom TabControl -->
            <controls:TabControlCustom Grid.Column="1"
                                      SelectedIndex="{Binding SelectedTabIndex}"
                                      HorizontalAlignment="Center">
                <controls:TabItemCustom Header="Tab1" IsSelected="True">
                    <TextBlock Text="Tab 1 Content" 
                              HorizontalAlignment="Center" 
                              VerticalAlignment="Center" />
                </controls:TabItemCustom>
                <controls:TabItemCustom Header="Tab2">
                    <TextBlock Text="Tab 2 Content" 
                              HorizontalAlignment="Center" 
                              VerticalAlignment="Center" />
                </controls:TabItemCustom>
            </controls:TabControlCustom>
            
            <!-- Right Button -->
            <Button Grid.Column="2" 
                   Content="Button"
                   Command="{Binding RightButtonCommand}"
                   Margin="10,0,0,0" />
        </Grid>

        <!-- Content Area -->
        <Border Grid.Row="1" 
                Background="White">
            <ContentControl Content="{Binding TabContent}" />
        </Border>
    </Grid>
</UserControl>
```

---

## 5. ViewModel với Dynamic Content

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Avalonia.Layout;

namespace camera.ViewModels;

public partial class ExamplePageViewModel : ViewModelBase
{
    [ObservableProperty]
    private int selectedTabIndex = 0;

    [ObservableProperty]
    private Control? tabContent;

    public ExamplePageViewModel()
    {
        UpdateTabContent();
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        UpdateTabContent();
    }

    private void UpdateTabContent()
    {
        TabContent = SelectedTabIndex switch
        {
            0 => CreateTab1Content(),
            1 => CreateTab2Content(),
            _ => new TextBlock { Text = "Unknown Tab" }
        };
    }

    private Control CreateTab1Content()
    {
        return new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 20,
            Children =
            {
                new TextBlock 
                { 
                    Text = "Tab 1 Content",
                    FontSize = 24,
                    FontWeight = Avalonia.Media.FontWeight.Bold
                },
                new TextBlock 
                { 
                    Text = "This is the content area for Tab 1.\nYou can put any controls here.",
                    TextAlignment = Avalonia.Media.TextAlignment.Center
                }
            }
        };
    }

    private Control CreateTab2Content()
    {
        return new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 20,
            Children =
            {
                new TextBlock 
                { 
                    Text = "Tab 2 Content",
                    FontSize = 24,
                    FontWeight = Avalonia.Media.FontWeight.Bold
                },
                new TextBlock 
                { 
                    Text = "This is the content area for Tab 2.\nYou can put any controls here.",
                    TextAlignment = Avalonia.Media.TextAlignment.Center
                }
            }
        };
    }

    [RelayCommand]
    private void LeftButton()
    {
        Console.WriteLine("Left button clicked");
        // Your logic here
    }

    [RelayCommand]
    private void RightButton()
    {
        Console.WriteLine("Right button clicked");
        // Your logic here
    }
}
```

---

## 6. Layout với Spacing và Alignment Tốt Hơn

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:camera.ViewModels"
             x:Class="camera.Views.ExamplePageView"
             x:DataType="vm:ExamplePageViewModel">
    
    <Grid RowDefinitions="Auto, *">
        <!-- Top Bar with proper spacing -->
        <Border Grid.Row="0" 
                Background="#F8F8F8" 
                BorderBrush="#E0E0E0"
                BorderThickness="0,0,0,1">
            <Grid ColumnDefinitions="Auto, *, Auto" 
                  Margin="20,15">
                
                <!-- Left Button -->
                <Button Grid.Column="0" 
                       Content="Button"
                       Command="{Binding LeftButtonCommand}"
                       Padding="20,10"
                       MinWidth="100" />
                
                <!-- TabControl centered -->
                <TabControl Grid.Column="1" 
                           SelectedIndex="{Binding SelectedTabIndex}"
                           HorizontalAlignment="Center"
                           VerticalAlignment="Center">
                    <TabItem Header="Tab1" Padding="30,10" />
                    <TabItem Header="Tab2" Padding="30,10" />
                </TabControl>
                
                <!-- Right Button -->
                <Button Grid.Column="2" 
                       Content="Button"
                       Command="{Binding RightButtonCommand}"
                       Padding="20,10"
                       MinWidth="100" />
            </Grid>
        </Border>

        <!-- Content Area -->
        <Border Grid.Row="1" 
                Background="White">
            <ScrollViewer>
                <ContentControl Content="{Binding TabContent}" 
                              Margin="20" />
            </ScrollViewer>
        </Border>
    </Grid>
</UserControl>
```

---

## 7. Responsive Layout

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:camera.ViewModels"
             x:Class="camera.Views.ExamplePageView"
             x:DataType="vm:ExamplePageViewModel">
    
    <Grid RowDefinitions="Auto, *">
        <!-- Top Bar - Responsive -->
        <Grid Grid.Row="0" 
              ColumnDefinitions="Auto, *, Auto"
              MinHeight="60"
              Background="#F5F5F5">
            
            <!-- Left Button -->
            <Button Grid.Column="0" 
                   Content="Button"
                   Command="{Binding LeftButtonCommand}"
                   Margin="15"
                   Padding="15,10"
                   HorizontalAlignment="Left"
                   VerticalAlignment="Center" />
            
            <!-- TabControl - Takes remaining space -->
            <TabControl Grid.Column="1" 
                       SelectedIndex="{Binding SelectedTabIndex}"
                       HorizontalAlignment="Center"
                       VerticalAlignment="Center"
                       Margin="10,0">
                <TabItem Header="Tab1" />
                <TabItem Header="Tab2" />
            </TabControl>
            
            <!-- Right Button -->
            <Button Grid.Column="2" 
                   Content="Button"
                   Command="{Binding RightButtonCommand}"
                   Margin="15"
                   Padding="15,10"
                   HorizontalAlignment="Right"
                   VerticalAlignment="Center" />
        </Grid>

        <!-- Content Area - Fills remaining space -->
        <ContentControl Grid.Row="1" 
                       Content="{Binding TabContent}"
                       Margin="20" />
    </Grid>
</UserControl>
```

---

## Tóm Tắt

### Cấu trúc Layout:

1. **Grid với 2 rows**: 
   - Row 0: Top bar (Auto height)
   - Row 1: Content area (* - fills remaining)

2. **Top Bar Grid với 3 columns**:
   - Column 0: Left Button (Auto width)
   - Column 1: TabControl (* - takes remaining space)
   - Column 2: Right Button (Auto width)

3. **Content Area**: 
   - ContentControl bind với ViewModel property
   - Thay đổi content dựa trên selected tab

### Key Points:

- ✅ Sử dụng `Grid` với `ColumnDefinitions` và `RowDefinitions`
- ✅ `Auto` cho buttons, `*` cho TabControl và content area
- ✅ `HorizontalAlignment="Center"` cho TabControl
- ✅ Bind `SelectedIndex` để track tab selection
- ✅ Dynamic content với `ContentControl`

Bạn có thể copy code này và customize theo nhu cầu! 🚀

