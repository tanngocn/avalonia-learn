using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace camera.ViewModels;

public partial class AddLivePrintDialogViewModel : DialogViewModel
{
    [ObservableProperty] private string _title = "Add Print View";
    [ObservableProperty] private string _message = "Something mess";
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _description;
    [ObservableProperty] private double _dialogWidth = double.NaN;

    [ObservableProperty] private bool _confirmed;
    [ObservableProperty] private string _confirmText = "Create";
    [ObservableProperty] private string _cancelText = "Cancel";

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _loading = false;
    public bool NotLoading() => !Loading;
    
    [ObservableProperty]
    private bool _allowAutoSave = false;
    
    public Func<AddLivePrintDialogViewModel, Task<bool>> OnSave { get; set; } = (_) =>
    {
        // To do something API
        return Task.FromResult(true);
    };

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (Loading) return;
        
        // Nếu đã cho phép auto save, không cần hỏi lại, save luôn
        if (!AllowAutoSave)
        {
            // Có thể thêm logic confirm ở đây nếu cần
            // Nhưng hiện tại không có confirm, nên chỉ cần check AllowAutoSave
        }
        
        Loading = true;
        var result = await OnSave(this);

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