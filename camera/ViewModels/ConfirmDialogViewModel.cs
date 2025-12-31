using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace camera.ViewModels;

public partial class ConfirmDialogViewModel: DialogViewModel
{
    [ObservableProperty] private string _title = "Confirm";
    [ObservableProperty] private string _message = "Message";
    [ObservableProperty] private string _confirmText = "Ok";
    [ObservableProperty] private string _cancelText = "Cancel";
    [ObservableProperty] private bool _confirmed;
    [ObservableProperty] private double _dialogWidth = double.NaN;
    [ObservableProperty] private string _name = "Confirm";
    [ObservableProperty] private string _description= "Confirm description";
    
    [ObservableProperty] 
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _loading = false;
    
    public bool  NotLoading()=> !Loading;

    // run task async await
    [JsonIgnore]
    public Func<ConfirmDialogViewModel, Task<bool>> OnConfirm { get; set; } = (_) =>
    {
        // To do something API
        return Task.FromResult(true);
    };
    
    [RelayCommand]
    public async Task ConfirmAsync()
    {
        if (Loading) return;
        Loading = true;
        var result = await OnConfirm(this);

        Loading = false;
        if (!result) return;
        
        Confirmed = true;
        Close();
    }
    
    [RelayCommand(CanExecute = nameof(NotLoading))]
    public async void Cancel()
    {
        IsDialogOpen = false;
        Close();
    }
    


}