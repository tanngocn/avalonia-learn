using CommunityToolkit.Mvvm.ComponentModel;

namespace camera.ViewModels;

public partial class TypeViewModel: ViewModelBase
{
    [ObservableProperty]
    private double _id;
    [ObservableProperty]
    private string _name;
}