using Avalonia;
using Avalonia.Headless;
using DinnersReady.Tests;

// Supply assembly and full typename in a single string format OR use [AvaloniaTestApplication]
[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace DinnersReady.Tests;

public class TestAppBuilder
{
    // Bootstraps your main App class (from DinnersReady/App.axaml.cs) in headless mode
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<DinnersReady.App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}