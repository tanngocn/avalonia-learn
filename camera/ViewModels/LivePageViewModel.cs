using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using camera.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Threading.Tasks;
using camera.Services;
using camera.Interfaces;

namespace camera.ViewModels;



public partial class LivePageViewModel(
    MainWindowViewModel mainWindowViewModel,
    DialogService dialogService,
    PrintService printService) : PageViewModel(ApplicationPageNames.Live)
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PrintListHasItems))]
    private ObservableCollection<LivePrintViewModel> _printList;

    [ObservableProperty] private LivePrintViewModel _selectedPrintListItem;
    [ObservableProperty] private string _name="React";

    [ObservableProperty] private ObservableCollection<KeyValuePair<string, string>> _printerSizeOptions =
    [
        new("0", "(Default)"),
        new("1", "(Default)")
    ];

    [ObservableProperty] private ObservableCollection<KeyValuePair<string, string>> _typeBook;

    [ObservableProperty] private KeyValuePair<string, string>? _selectedTypeBookItem;

    // Auto save setting - lưu trạng thái cho phép auto save
    [ObservableProperty]
    private bool _allowAutoSave = false;

    // Sidebar state
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSidebarVisible))]
    [NotifyPropertyChangedFor(nameof(IsDeviceButtonVisible))]
    private bool _isSidebarOpen = true; // Default mở sidebar

    public bool IsSidebarVisible => IsSidebarOpen;
    public bool IsDeviceButtonVisible => !IsSidebarOpen;

    private bool _isUpdatingSelection;

    partial void OnTypeBookChanged(ObservableCollection<KeyValuePair<string, string>> value)
    {
        // Set selected item when TypeBook is loaded
        if (value != null && !string.IsNullOrEmpty(Name) && !_isUpdatingSelection)
        {
            var matchingItem = value.FirstOrDefault(x => x.Value == Name);
            if (!matchingItem.Equals(default(KeyValuePair<string, string>)))
            {
                _isUpdatingSelection = true;
                SelectedTypeBookItem = matchingItem;
                _isUpdatingSelection = false;
            }
        }
    }

    partial void OnNameChanged(string value)
    {
        // Update selected item when Name changes (but not if we're updating from SelectedTypeBookItem)
        if (TypeBook != null && !string.IsNullOrEmpty(value) && !_isUpdatingSelection)
        {
            var matchingItem = TypeBook.FirstOrDefault(x => x.Value == value);
            if (!matchingItem.Equals(default(KeyValuePair<string, string>)) && 
                (!SelectedTypeBookItem.HasValue || SelectedTypeBookItem.Value.Value != value))
            {
                _isUpdatingSelection = true;
                SelectedTypeBookItem = matchingItem;
                _isUpdatingSelection = false;
            }
        }
    }

    partial void OnSelectedTypeBookItemChanged(KeyValuePair<string, string>? value)
    {
        // Update Name when selected item changes
        if (value.HasValue && Name != value.Value.Value)
        {
            _isUpdatingSelection = true;
            Name = value.Value.Value;
            _isUpdatingSelection = false;
        }
    }


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
        // GET get type from service here
        var typeBook = printService.GetTypeBook();
        var typeOptions =
            new ObservableCollection<KeyValuePair<string, string>>(typeBook.Select((f, i) =>
                new KeyValuePair<string, string>(f.Id.ToString(), f.Name)));
        TypeBook = typeOptions;
        //End get type
        // Todo Fetch from services
        PrintList =
        [
            new LivePrintViewModel { Name = "react", Description = "React Description" },
            new LivePrintViewModel { Name = "angular", Description = "Angular Description" },
            new LivePrintViewModel { Name = "C#", Description = "C# description" },
        ];

        // Update PrintListItems when collections changes
        PrintList.CollectionChanged += (_, _) => OnPropertyChanged(nameof(PrintListHasItems));

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

        var printItemViewModel = PrintList.FirstOrDefault(f => f.Name == name);

        var copiedPrintViewModel = new EditLivePrintDialogViewModel();
        copiedPrintViewModel.RestoreState(printItemViewModel?.GetState());
        
        // Set AllowAutoSave từ setting đã lưu
        copiedPrintViewModel.AllowAutoSave = AllowAutoSave;

        await dialogService.ShowDialog(mainWindowViewModel, copiedPrintViewModel);

        if (!copiedPrintViewModel.Confirmed) return;
        
        // Lưu trạng thái AllowAutoSave nếu user đã thay đổi
        if (copiedPrintViewModel.AllowAutoSave != AllowAutoSave)
        {
            AllowAutoSave = copiedPrintViewModel.AllowAutoSave;
        }
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

        if (index > 0) index--;

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
        
        // Set AllowAutoSave từ setting đã lưu
        addPrintItemViewModel.AllowAutoSave = AllowAutoSave;

        await dialogService.ShowDialog(mainWindowViewModel, addPrintItemViewModel);

        if (!addPrintItemViewModel.Confirmed) return;
        
        // Lưu trạng thái AllowAutoSave nếu user đã thay đổi
        if (addPrintItemViewModel.AllowAutoSave != AllowAutoSave)
        {
            AllowAutoSave = addPrintItemViewModel.AllowAutoSave;
        }

        var newItem = new LivePrintViewModel
            { Name = Guid.NewGuid().ToString("N"), Description = "C++ descirption", IsSelected = true };

        PrintList.Add(newItem);
    }

    [RelayCommand]
    private void OpenSidebar()
    {
        IsSidebarOpen = true;
    }

    [RelayCommand]
    private void CloseSidebar()
    {
        IsSidebarOpen = false;
    }

    /// <summary>
    /// Tạo tab mới từ device node được drag & drop
    /// Trả về thông tin để tạo content, không tạo control instance ngay
    /// </summary>
    public (string TabHeader, string DeviceName, bool IsParent) CreateTabInfoFromDeviceNode(string deviceName, bool isParent)
    {
        return (deviceName, deviceName, isParent);
    }

    /// <summary>
    /// Tạo control content từ thông tin tab
    /// </summary>
    public Control CreateTabContent(string deviceName, bool isParent)
    {
        if (isParent)
        {
            // Node cha → CameraListView
            var listViewModel = new CameraListViewModel(deviceName);
            return new Views.CameraListView
            {
                DataContext = listViewModel
            };
        }
        else
        {
            // Node con → CameraDetailView
            var detailViewModel = new CameraDetailViewModel(deviceName);
            return new Views.CameraDetailView
            {
                DataContext = detailViewModel
            };
        }
    }

    protected override void OnDesignTimeContructor() => FetchPrintList();
}