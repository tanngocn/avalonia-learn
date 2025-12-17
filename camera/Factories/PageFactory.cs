using System;
using camera.Data;
using camera.ViewModels;

namespace camera.Factories;

public class PageFactory(Func<ApplicationPageNames, PageViewModel> factory)
{
    public PageViewModel GetPageViewModel(ApplicationPageNames pageName)=> factory.Invoke(pageName);
}