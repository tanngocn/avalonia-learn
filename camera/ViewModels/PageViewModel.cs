using camera.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace camera.ViewModels;

public partial class PageViewModel(ApplicationPageNames pageName) : ViewModelBase
{
    [ObservableProperty] private ApplicationPageNames _pageName = pageName;
}