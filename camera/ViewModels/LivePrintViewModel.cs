using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace camera.ViewModels;

public partial class LivePrintViewModel: ViewModelBase
{
    [property: JsonIgnore]
    private string _savedState = "";
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanged))]
    private string _name;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChanged))]
    private string _description;
    
    [ObservableProperty]
    private bool _isNewItem;
    
    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isSelected;
    


    [property: JsonIgnore]
    public new bool  HasChanged => IsNewItem ||  SavedState != ""  && SavedState != JsonSerializer.Serialize(this);

    public void SetSavedState(){
        _savedState= JsonSerializer.Serialize(this);

        OnPropertyChanged(nameof(HasChanged));
    }
    // discard function
    public void RestoreSavedState()
    {
        var savedState = JsonSerializer.Deserialize<LivePrintViewModel>(_savedState);

        foreach (var propertyInfo in GetType().GetProperties())
        {
            // only set setters, not get only properites
            if (!propertyInfo.CanWrite)
                continue;
            
            // Ignore any properties that have a JsonIgnore attribute
            if (propertyInfo.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).GetLength(0) > 0) continue;
            
            // pull the saved value
            var originValue = propertyInfo.GetValue(savedState);
            
            // restore it to this class
            propertyInfo.SetValue(this, originValue);
        }
    }
}