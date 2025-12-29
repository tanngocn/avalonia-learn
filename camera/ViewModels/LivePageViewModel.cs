using System;
using System.Collections.ObjectModel;
using camera.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Threading.Tasks;
using camera.Services;
using camera.Interfaces;

namespace camera.ViewModels;

public partial class LivePageViewModel(MainWindowViewModel mainWindowViewModel, DialogService dialogService) : PageViewModel(ApplicationPageNames.Live)
{
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(PrintListHasItems))]
    private ObservableCollection<LivePrintViewModel> _printList;

    [ObservableProperty] private LivePrintViewModel _selectedPrintListItem;


    [RelayCommand]
    public void RefreshLivePage(LiveTabNames liveTabNames)
    {
        switch (liveTabNames)
        {
            case LiveTabNames.Print: FetchPrintList(); break;
        }
    }

    public bool PrintListHasItems => PrintList.Any();
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
        
        // Update PrintListItems when collections changes
        PrintList.CollectionChanged+= (_,_) => OnPropertyChanged(nameof(PrintListHasItems));
        
        // choose first item;
        if (PrintList.Count > 0)
        {
            //Select first item
            PrintList.First().IsSelected = true;

            foreach (var printItem in PrintList)
            {
                printItem.SetSavedState();
            }
        }
    }
    [RelayCommand]
    public async Task DeletePrintList(string title)
    {
        if (PrintList.Count(x => x.Title == title) != 1)
        {
            //throw error
            return;
        }

        var confirmViewModel = new ConfirmDialogViewModel
        {
            Title = "Confirm",
            Descirption = "Test config Dialog"
        };
        
        await dialogService.ShowDialog(mainWindowViewModel, confirmViewModel);
        
        if (!confirmViewModel.Confirmed) return;
        

        var index = PrintList.IndexOf((PrintList.First(x => x.Title == title)));
        PrintList.RemoveAt(index);

        if (index > 0) index --;

        if (PrintList.Count > 0) PrintList[index].IsSelected = true;


    }

    [RelayCommand]
    public void AddPrintItem()
    {
        var newItem = new LivePrintViewModel { Title = Guid.NewGuid().ToString("N"), Description = "C++ descirption", IsSelected = true };

        PrintList.Add(newItem);
    }

    protected override void OnDesignTimeContructor() => FetchPrintList();
}