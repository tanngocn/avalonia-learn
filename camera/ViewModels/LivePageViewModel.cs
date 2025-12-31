using System;
using System.Collections.Generic;
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
    
    [ObservableProperty] 
    private ObservableCollection<KeyValuePair<string, string>> _printerSizeOptions = [
    
        new("0", "(Default)"),
        new("1", "(Default)")];


    [RelayCommand]
    public void RefreshLivePage(LiveTabNames liveTabNames)
    {
        switch (liveTabNames)
        {
            case LiveTabNames.Print: FetchPrintList(); break;
        }
    }

    public bool PrintListHasItems => PrintList.Any();
    private void FetchPrintList()
    {
        // Todo Fetch from services
        PrintList =
        [
            new LivePrintViewModel { Name = "react", Description = "React Description" },
            new LivePrintViewModel { Name = "angular", Description = "Angular Description" },
            new LivePrintViewModel { Name = "C#", Description = "C# description" },
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
    public async Task EditPrintItem(string name)
    {
        if (PrintList.Count(x => x.Name == name) != 1)
        {
            //throw error
            return;
        }
        var printItemViewModel = PrintList.FirstOrDefault(f=> f.Name == name);

        var copiedPrintViewModel = new EditLivePrintDialogViewModel();
        copiedPrintViewModel.RestoreState(printItemViewModel?.GetState());
        
        await dialogService.ShowDialog(mainWindowViewModel, copiedPrintViewModel);
        
        if (!copiedPrintViewModel.Confirmed) return;

    }

    [RelayCommand]
    public async Task DeletePrintList(string name)
    {
        if (PrintList.Count(x => x.Name == name) != 1)
        {
            //throw error
            return;
        }

        var confirmViewModel = new ConfirmDialogViewModel
        {
            Title = "Confirm",
            Message = "Test config Dialog"
        };
        
        await dialogService.ShowDialog(mainWindowViewModel, confirmViewModel);
        
        if (!confirmViewModel.Confirmed) return;
        

        var index = PrintList.IndexOf((PrintList.First(x => x.Name == name)));
        PrintList.RemoveAt(index);

        if (index > 0) index --;

        if (PrintList.Count > 0) PrintList[index].IsSelected = true;


    }

    [RelayCommand]
    public async Task AddPrintItem()
    {
        
        var addPrintItemViewModel = new AddLivePrintDialogViewModel
        {
            Title = "Add Print",
            Message = "Test config Dialog"
        };
        
        await dialogService.ShowDialog(mainWindowViewModel, addPrintItemViewModel);
        
        if (!addPrintItemViewModel.Confirmed) return;

        var newItem = new LivePrintViewModel { Name = Guid.NewGuid().ToString("N"), Description = "C++ descirption", IsSelected = true };

        PrintList.Add(newItem);
    }

    protected override void OnDesignTimeContructor() => FetchPrintList();
}