using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace camera.ViewModels;

public partial class LivePrintViewModel: ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanged))]
    private string _title;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanged))]
    private string _description;
    
    [ObservableProperty]
    private bool _isSelected;
    
    [JsonIgnore]
    private string _savedState = "";

    [JsonIgnore]
    public bool  HasChanged => _savedState != JsonSerializer.Serialize(this);

    public void SetSavedState(){
        _savedState= JsonSerializer.Serialize(this);

        OnPropertyChanged(nameof(HasChanged));
    }
}