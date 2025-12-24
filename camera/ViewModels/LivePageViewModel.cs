using camera.Data;

namespace camera.ViewModels;

public partial class LivePageViewModel() : PageViewModel(ApplicationPageNames.Live)
{
    public string Test { get; set; } = "LivePage";
}