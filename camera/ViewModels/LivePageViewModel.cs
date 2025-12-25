using System.Collections.ObjectModel;
using camera.Data;
using CommunityToolkit.Mvvm.Input;

namespace camera.ViewModels;

public partial class LivePageViewModel() : PageViewModel(ApplicationPageNames.Live)
{
    public string Test { get; set; } = "LivePage";
    private ObservableCollection<LivePrintViewModel> _printList;

    public void RefreshLivePage(LiveTabNames liveTabNames)
    {
        switch (liveTabNames)
        {
                case LiveTabNames.Print: FetchPrintList(); break;
        }
    }
    [RelayCommand]
    public void FetchPrintList()
    {
        // Todo Fetch from services
        _printList =
        [
            new LivePrintViewModel { Title = "react", Description = "React Description" },
            new LivePrintViewModel { Title = "angular", Description = "Angular Description" },
            new LivePrintViewModel { Title = "C#", Description = "C# description" },
        ];
    }
}