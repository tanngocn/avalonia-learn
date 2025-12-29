using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace camera.ViewModels;

public partial class DialogViewModel:ViewModelBase
{
    [ObservableProperty] private bool _isDialogOpen;
    protected TaskCompletionSource closeTask = new TaskCompletionSource();

    public async Task WaitAsync()
    {
        await closeTask.Task;
    }

    public void Show()
    {
        if(closeTask.Task.IsCompleted)
            closeTask = new TaskCompletionSource();
        IsDialogOpen = true;
    }

    public void Close()
    {
        IsDialogOpen = false;
        closeTask.TrySetResult();
    }
}