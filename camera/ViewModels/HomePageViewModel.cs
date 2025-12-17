using camera.Data;

namespace camera.ViewModels;

public partial class HomePageViewModel: PageViewModel
{
    public string Test { get; set; } = "HomePage";
    public HomePageViewModel()
    {
        PageName = ApplicationPageNames.Home;
    }
}