using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using camera.Data;
using camera.ViewModels;

namespace camera.Views;

public partial class LivePageView : UserControl
{
    public LivePageView()
    {
        InitializeComponent();
    }

    private void LiveTab_OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => OnTabChanged();

    private void OnTabChanged()
    {
        var selectedTab = (LiveTabControls?.SelectedItem  as TabItem)!.Content! as Control;
        
        if (selectedTab == null) return;
        
        var liveTabName = selectedTab switch
        {
            LivePrintView => LiveTabNames.Print, _ => LiveTabNames.Unknown,
        };
        var viewModel = selectedTab.DataContext as LivePageViewModel;

        viewModel?.RefreshLivePage(liveTabName);
    }

    protected override void OnInitialized()
    {
        OnTabChanged();
        base.OnInitialized();
    }
}