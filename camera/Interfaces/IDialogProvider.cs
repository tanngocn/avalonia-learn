using camera.ViewModels;

namespace camera.Interfaces;

public interface IDialogProvider
{
    DialogViewModel Dialog { get; set; }
}