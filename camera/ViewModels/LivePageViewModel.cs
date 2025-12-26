using System;
using System.Collections.ObjectModel;
using camera.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;

namespace camera.ViewModels;

public partial class LivePageViewModel() : PageViewModel(ApplicationPageNames.Live)
{
    [ObservableProperty] private ObservableCollection<LivePrintViewModel> _printList;

    [ObservableProperty] private LivePrintViewModel _selectedPrintListItem;


    [RelayCommand]
    public void RefreshLivePage(LiveTabNames liveTabNames)
    {
        switch (liveTabNames)
        {
            case LiveTabNames.Print: FetchPrintList(); break;
        }
    }

    [RelayCommand]
    private void FetchPrintList()
    {
        // Todo Fetch from services
        PrintList =
        [
            new LivePrintViewModel { Title = "react", Description = "React Description" },
            new LivePrintViewModel { Title = "angular", Description = "Angular Description" },
            new LivePrintViewModel { Title = "C#", Description = "C# description" },
        ];
    }

    [RelayCommand]
    public void DeletePrintList(string title)
    {
        if (PrintList.Count(x => x.Title == title) != 1)
        {
            //throw error
            return;
        }

        PrintList.Remove((PrintList.First(x => x.Title == title)));
    }

    [RelayCommand]
    public void AddPrintItem()
    {
        var newItem = new LivePrintViewModel { Title = "C++", Description = "C++ descirption", IsSelected = true };

        PrintList.Add(newItem);
    }

    protected override void OnDesignTimeContructor() => FetchPrintList();
}