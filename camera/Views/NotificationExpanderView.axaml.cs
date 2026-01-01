using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using camera.ViewModels;

namespace camera.Views;

public partial class NotificationExpanderView : UserControl
{
    public NotificationExpanderView()
    {
        InitializeComponent();
        // Set DataContext nếu chưa có
        if (DataContext == null)
        {
            DataContext = new NotificationExpanderViewModel();
        }
    }

    private async void DismissButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is NotificationItem notification)
        {
            var viewModel = DataContext as NotificationExpanderViewModel;
            if (viewModel != null)
            {
                await viewModel.DismissCommand.ExecuteAsync(notification);
            }
        }
    }
}

