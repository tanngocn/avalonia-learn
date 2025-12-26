using camera.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace camera.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    [ObservableProperty] private ApplicationPageNames _pageName;

    protected PageViewModel(ApplicationPageNames pageName)
    {
        _pageName = pageName;
        //detect design time
        if (Avalonia.Controls.Design.IsDesignMode)
        {
            OnDesignTimeContructor();
        }
    }

    protected virtual void OnDesignTimeContructor(){ }
}