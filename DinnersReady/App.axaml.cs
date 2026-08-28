using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DinnersReady.Services;
using DinnersReady.ViewModels;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net.Http;

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

    public IServiceProvider? Services { get; private set; }

#pragma warning disable CA1416 // Validate platform compatibility
    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        var apiKey = "ENTER_GOOGLE_API_KEY";
        // Create an HttpClient with Google's required header
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://generativelanguage.googleapis.com/v1beta/openai"),
            Transport = new HttpClientPipelineTransport(httpClient)
        };

        // Register Services & ViewModels
        services.AddSingleton<IIngredientStoreRepository, IngredientStoreRepository>();
        services.AddSingleton<IngredientStore>();
        services.AddSingleton<IChatClient>(sp => new GeminiChatClient(apiKey, "gemini-3.6-flash"));

        services.AddTransient<RecipeGeneratorService>();
        services.AddTransient<RecipeGeneratorViewModel>();
        services.AddTransient<MainViewModel>();

        // Register Views for each platform profile
        services.AddTransient<Views.FullSize.MainWindow>(); // Desktop Window Shell
        services.AddTransient<Views.Web.MainView>();     // Browser UserControl
        services.AddTransient<Views.Mobile.MainView>();     // Mobile UserControl

        // Build container once
        Services = services.BuildServiceProvider();

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