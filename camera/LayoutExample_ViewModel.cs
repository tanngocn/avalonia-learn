// ============================================
// VIEWMODEL CODE - COMPLETE EXAMPLE
// Copy-paste vào ViewModel của bạn
// ============================================

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
        System.Diagnostics.Debug.WriteLine("Left button clicked");
        // Your logic here
    }

    [RelayCommand]
    private void RightButton()
    {
        System.Diagnostics.Debug.WriteLine("Right button clicked");
        // Your logic here
    }
}

// ============================================
// SIMPLER VERSION (Nếu không cần dynamic content)
// ============================================

/*
public partial class ExamplePageViewModel : ViewModelBase
{
    [ObservableProperty]
    private int selectedTabIndex = 0;

    [RelayCommand]
    private void LeftButton()
    {
        Console.WriteLine("Left button clicked");
    }

    [RelayCommand]
    private void RightButton()
    {
        Console.WriteLine("Right button clicked");
    }
}
*/

// ============================================
// VERSION VỚI USERCONTROL CONTENT
// ============================================

/*
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
            0 => new Views.Tab1ContentView(), // Your custom UserControl
            1 => new Views.Tab2ContentView(), // Your custom UserControl
            _ => new TextBlock { Text = "Unknown Tab" }
        };
    }

    [RelayCommand]
    private void LeftButton()
    {
        Console.WriteLine("Left button clicked");
    }

    [RelayCommand]
    private void RightButton()
    {
        Console.WriteLine("Right button clicked");
    }
}
*/

