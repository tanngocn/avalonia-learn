using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace camera.ViewModels;

public partial class ConfirmDialogViewModel: DialogViewModel
{
    [ObservableProperty] private string _title = "Confirm";
    [ObservableProperty] private string _descirption= "Confirm description";
    [ObservableProperty] private string _confirmText = "Ok";
    [ObservableProperty] private string _cancelText = "Cancel";
    [ObservableProperty] private bool _confirmed;
    
    [RelayCommand]
    public void Confirm()
    {
        IsDialogOpen = true;
        Close();
    }
    
    [RelayCommand]
    public void Cancel()
    {
        IsDialogOpen = false;
        Close();
    }
    


}