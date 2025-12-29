using System.Threading.Tasks;
using camera.Interfaces;
using camera.ViewModels;

namespace camera.Services;

public class DialogService
{
    public async Task ShowDialog<THost, TDialogViewModel>(THost host ,TDialogViewModel dialogViewModel)
        where TDialogViewModel: DialogViewModel
        where THost: IDialogProvider
    {
        host.Dialog = dialogViewModel;
        dialogViewModel.Show();

        //Wait dialog close
        await dialogViewModel.WaitAsync();
    }
}