using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DinnersReady.Services;
using DinnersReady.ViewModels;
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

        // Register Services & ViewModels
        collection.AddSingleton<IIngredientStoreRepository, IngredientStoreRepository>();
        collection.AddSingleton<IngredientStore>();
        collection.AddTransient<MainViewModel>();

        // Register Views for each platform profile
        collection.AddTransient<Views.FullSize.MainWindow>(); // Desktop Window Shell
        collection.AddTransient<Views.Web.MainView>();     // Browser UserControl
        collection.AddTransient<Views.Mobile.MainView>();     // Mobile UserControl

        // Build container once
        Services = collection.BuildServiceProvider();

        var mainVm = Services.GetRequiredService<MainViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Desktop (Windows/macOS/Linux)
            desktop.MainWindow = new Views.FullSize.MainWindow
            {
                DataContext = mainVm
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
            {
                // Mobile UI (Android & iOS)
                singleView.MainView = new Views.Mobile.MainView
                {
                    DataContext = mainVm
                };
            }
            else
            {
                // Web / Browser UI (WASM)
                singleView.MainView = new Views.Web.MainView
                {
                    DataContext = mainVm
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
#pragma warning restore CA1416
}