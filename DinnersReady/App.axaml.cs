using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DinnersReady.Services;
using DinnersReady.ViewModels;
using DinnersReady.Views;
using Microsoft.Extensions.DependencyInjection;

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

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();

        // Register Repositories and Services
        collection.AddSingleton<IIngredientStoreRepository, IngredientStoreRepository>();
        collection.AddSingleton<IngredientStore>();

        // Register ViewModels as Transient
        collection.AddTransient<MainViewModel>();
        collection.AddTransient<MainView>();

        Services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            // Resolve directly inside the property setter
            singleView.MainView = new MainView
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime activity)
        {
            activity.MainViewFactory = () => new MainView
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}