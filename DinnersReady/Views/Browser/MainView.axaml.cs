using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace DinnersReady.Views.Browser;

[SupportedOSPlatform("browser")]
public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        Loaded += OnMainViewLoaded;
    }

    private void OnMainViewLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Loaded -= OnMainViewLoaded;

        // Defer invocation until after the initial UI rendering/layout pass finishes
        Dispatcher.UIThread.Post(() =>
        {
            if (OperatingSystem.IsBrowser())
            {
                DismissLoader();
            }
        }, DispatcherPriority.Loaded);
    }

    [JSImport("globalThis.hideAppLoader")]
    private static partial void DismissLoader();
}