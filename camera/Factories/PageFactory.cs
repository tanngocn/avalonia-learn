using System;
using camera.Data;
using camera.ViewModels;

namespace camera.Factories;

public class PageFactory(Func<ApplicationPageNames, PageViewModel> factory)
{
    public PageViewModel GetPageViewModel(ApplicationPageNames pageName)=> factory.Invoke(pageName);
}
// public class PageFactory(Func<Type, PageViewModel> factory)
// {
//     public PageViewModel GetPageViewModel<T>(Action<T> afterCreation = null)
//     where T : PageViewModel
//     {
//         var viewModel = factory(typeof(T));
//         afterCreation?.Invoke((T)viewModel);
//         return viewModel;
//     }
// }