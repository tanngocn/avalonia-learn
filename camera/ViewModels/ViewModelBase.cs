using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace camera.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        IgnoreReadOnlyFields = false,
        IgnoreReadOnlyProperties = false,
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    public string SavedState = "";

    [JsonIgnore]
    public virtual bool HasChanged => SavedState != "" && SavedState != JsonSerializer.Serialize(this, _jsonOptions);

    public void SetSavedState()
    {
        SavedState = GetState();
        OnPropertyChanged(nameof(HasChanged));
    }
    
    public  void ViewModalBase()
    {
        if (Avalonia.Controls.Design.IsDesignMode)
      
            OnDesignTimeContructor();
    }



    protected virtual void OnDesignTimeContructor()
    {
    }

    public string GetState() => JsonSerializer.Serialize(this, GetType().DeclaringType ?? GetType(), _jsonOptions);

    public void RestoreState(string? stateToRestore = null)
    {
        stateToRestore ??= SavedState;
        var type = GetType().DeclaringType ?? GetType();

        var savedState = JsonSerializer.Deserialize(stateToRestore, type, _jsonOptions);


        foreach (var propertyInfo in type.GetProperties())
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