using camera.Data;
using camera.Factories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace camera.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private PageFactory _pageFactory;
    
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(HomePageIsActive))]
    [NotifyPropertyChangedFor(nameof(LivePageIsActive))]
    private PageViewModel _currentPage;

    public bool HomePageIsActive => CurrentPage.PageName == ApplicationPageNames.Home;
    public bool LivePageIsActive => CurrentPage.PageName == ApplicationPageNames.Live;

    public MainWindowViewModel(PageFactory pageFactory) 
    {
        _pageFactory = pageFactory;
        GotoHome();
    }

    [RelayCommand]
    private void GotoHome() => CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Home);
    [RelayCommand]
    private void GotoLive() => CurrentPage = _pageFactory.GetPageViewModel(ApplicationPageNames.Live);
}