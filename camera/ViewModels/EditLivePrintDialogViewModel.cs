using CommunityToolkit.Mvvm.ComponentModel;

namespace camera.ViewModels;

public partial class EditLivePrintDialogViewModel: AddLivePrintDialogViewModel
{
    [ObservableProperty] private string _title = "Add Print View";
    [ObservableProperty] private string _message = "Something mess";
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _description;
    [ObservableProperty] private double _dialogWidth = double.NaN;
}