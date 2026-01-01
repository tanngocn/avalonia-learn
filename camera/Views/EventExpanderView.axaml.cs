using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using camera.ViewModels;

namespace camera.Views;

public partial class EventExpanderView : UserControl
{
    public EventExpanderView()
    {
        InitializeComponent();
        // Set DataContext nếu chưa có
        if (DataContext == null)
        {
            DataContext = new EventExpanderViewModel();
        }
    }
}

