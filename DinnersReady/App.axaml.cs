using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DinnersReady.Services;
using DinnersReady.ViewModels;
using DinnersReady.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DinnersReady;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG && !BROWSER
        this.AttachDeveloperTools();
#endif
    }

    public System.IServiceProvider? Services { get; private set; }

#pragma warning disable CA1416 // Validate platform compatibility
    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();

        collection.AddSingleton<IIngredientStoreRepository, IngredientStoreRepository>();
        collection.AddSingleton<IngredientStore>();
        collection.AddTransient<MainViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Register ViewModels as Transient
            collection.AddTransient<Views.Desktop.MainWindow>();

            Services = collection.BuildServiceProvider();
            desktop.MainWindow = new Views.Desktop.MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            collection.AddTransient<Views.Browser.MainView>();

            Services = collection.BuildServiceProvider();
            
            singleView.MainView = new Views.Browser.MainView
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime activity)
        {
            collection.AddTransient<Views.Mobile.MainView>();

            Services = collection.BuildServiceProvider();

            activity.MainViewFactory = () => new Views.Mobile.MainView
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
#pragma warning restore CA1416
}