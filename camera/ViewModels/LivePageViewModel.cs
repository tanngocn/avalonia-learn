using camera.Data;

namespace camera.ViewModels;

public partial class LivePageViewModel: PageViewModel
{
    public string Test { get; set; } = "LivePage";

    public LivePageViewModel()
    {
        PageName = ApplicationPageNames.Live;
    }
}