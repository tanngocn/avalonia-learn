using System.Collections.ObjectModel;
using camera.ViewModels;

namespace camera.Services;

public class PrintService
{
    public ObservableCollection<TypeViewModel> GetTypeBook()
    {
        var types = new ObservableCollection<TypeViewModel>();
        types.Add(new TypeViewModel{Id=0, Name="(Default)"});
        types.Add(new TypeViewModel{Id=1, Name="Dev"});
        types.Add(new TypeViewModel{Id=2, Name="Analyzer"});
        types.Add(new TypeViewModel{Id=3, Name="React"});
        return types;
    }
}