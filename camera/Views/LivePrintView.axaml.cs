using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using camera.ViewModels;

namespace camera.Views;

public partial class LivePrintView : UserControl
{
    public LivePrintView()
    {
        InitializeComponent();
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems == null) return;
        
        if (e.AddedItems?.Count >0  && e.AddedItems[0]  is LivePrintViewModel viewModel)
        {   
            //listen changed
            viewModel.SetSavedState();
        }
    }
}