using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DinnersReady.Services;
using DinnersReady.ViewModels;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using System;
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
        var apiKey = ConfigService.GetGeminiApiKey();

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
        _ = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://generativelanguage.googleapis.com/v1beta/openai"),
            Transport = new HttpClientPipelineTransport(httpClient)
        };

        SetUpServices(services, apiKey);

        Services = services.BuildServiceProvider();

        var mainVm = Services.GetRequiredService<MainViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Views.FullSize.MainWindow
            {
                DataContext = mainVm
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
            {
                var mobileView = new Views.Mobile.MainView
                {
                    DataContext = mainVm
                };
                singleView.MainView = mobileView;
            }
            else
            {
                var webView = new Views.Web.MainView
                {
                    DataContext = mainVm
                };
                singleView.MainView = webView;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void SetUpServices(ServiceCollection services, string apiKey)
    {
        // 1. Storage Provider Registration (Platform-specific)
        if (OperatingSystem.IsBrowser())
        {
            services.AddSingleton<IStorageProvider, WebLocalStorageProvider>();
        }
        else
        {
            services.AddSingleton<IStorageProvider, FileSystemStorageProvider>();
        }

        // 2. Repositories & Application Services
        services.AddSingleton<IIngredientStoreRepository, IngredientStoreRepository>();
        services.AddSingleton<IIngredientStoreService, IngredientStore>();
        services.AddSingleton<IRecipeStoreRepository, RecipeStoreRepository>();
        services.AddSingleton<IRecipeStoreService, RecipeStore>();

        // 3. AI Chat Services
        services.AddSingleton<IChatClient>(sp => new GeminiChatClient(apiKey, "gemini-3.6-flash"));
        services.AddSingleton<RecipeGeneratorService>();

        // 4. ViewModels & Contexts
        services.AddTransient<RecipeGeneratorContext>();
        services.AddTransient<RecipeGeneratorViewModel>();
        services.AddSingleton<MainServicesContext>();

        // 5. Platform Share Service & Views
        services.AddShareService();
        services.AddTransient<MainViewModel>();
        services.AddTransient<Views.FullSize.MainWindow>();
        services.AddTransient<Views.Web.MainView>();
        services.AddTransient<Views.Mobile.MainView>();
    }
#pragma warning restore CA1416
}