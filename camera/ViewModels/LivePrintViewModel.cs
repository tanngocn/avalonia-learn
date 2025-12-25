using CommunityToolkit.Mvvm.ComponentModel;

namespace camera.ViewModels;

public partial class LivePrintViewModel: ViewModelBase
{
    [ObservableProperty]
    private string _title;
    [ObservableProperty]
    private string _description;
}