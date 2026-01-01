using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.Metadata;

using camera.Data;
using camera.Factories;
using camera.Interfaces;
using camera.Services;
using camera.ViewModels;
using camera.Views;

using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;

[assembly: XmlnsDefinition("https://github.com/avaloniaui", "camera.Controls")]
namespace camera;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Core.Initialize();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();

        collection.AddSingleton(sp => new LibVLC(
            // "--avcodec-hw=any",
            "--avcodec-hw=none",
            "--no-video-title-show",
            "--no-snapshot-preview"
        ));
        
        collection.AddSingleton<MainWindowViewModel>();
        // Register MainWindowViewModel as IDialogProvider
        collection.AddTransient<HomePageViewModel>();
        collection.AddTransient<LivePageViewModel>();
        
        collection.AddSingleton<Func<ApplicationPageNames, PageViewModel>>(x => name => name switch
        {
            ApplicationPageNames.Home => x.GetRequiredService<HomePageViewModel>(),
            ApplicationPageNames.Live => x.GetRequiredService<LivePageViewModel>(),
        });
        // collection.AddSingleton<Func<Type, PageViewModel>>(x => type => type switch
        // {
        //     _ when type == typeof(HomePageViewModel) => x.GetRequiredService<HomePageViewModel>(),
        //     _ when type == typeof(LivePageViewModel) => x.GetRequiredService<LivePageViewModel>(),
        //     _=>  throw new  NotImplementedException()
        // });

        collection.AddSingleton<PageFactory>();
        collection.AddSingleton<DialogService>();
        collection.AddTransient<PrintService>();
        collection.AddSingleton<MediaFactory>();

        var services = collection.BuildServiceProvider();
      
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = services.GetRequiredService<MainWindowViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}