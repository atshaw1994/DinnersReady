using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

#if WINDOWS
    using Windows.ApplicationModel.DataTransfer;
#elif MACOS
    using AppKit;
    using Foundation;
#elif ANDROID || IOS
    using Microsoft.Maui.ApplicationModel.DataTransfer;
#endif

namespace DinnersReady.Services;

public interface IShareService
{
    Task ShareTextAsync(string title, string text);
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShareService(this IServiceCollection services)
    {
        if (OperatingSystem.IsWindows())
        {
#if WINDOWS
            services.AddSingleton<IShareService, WindowsShareService>();
#else
            services.AddSingleton<IShareService, DesktopFallbackShareService>();
#endif
        }
        else if (OperatingSystem.IsMacOS())
        {
#if MACOS
            services.AddSingleton<IShareService, MacOsShareService>();
#else
            services.AddSingleton<IShareService, DesktopFallbackShareService>();
#endif
        }
        else if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
        {
#if ANDROID || IOS
            services.AddSingleton<IShareService, MobileShareService>();
#else
            services.AddSingleton<IShareService, DesktopFallbackShareService>();
#endif
        }
        else
        {
            services.AddSingleton<IShareService, DesktopFallbackShareService>();
        }

        return services;
    }
}

#if WINDOWS
    public class WindowsShareService : IShareService
    {
        public Task ShareTextAsync(string title, string text)
        {
            var tcs = new TaskCompletionSource();

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += (s, e) =>
                {
                    var request = e.Request;
                    request.Data.Properties.Title = title;
                    request.Data.SetText(text);
                    tcs.SetResult();
                };
                DataTransferManager.ShowShareUI();
            });

            return tcs.Task;
        }
    }
#endif

#if MACOS
    public class MacOsShareService : IShareService
    {
        public Task ShareTextAsync(string title, string text)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var items = new NSObject[] { (NSString)$"{title}\n\n{text}" };
                var picker = new NSSharingServicePicker(items);

                var keyWindow = NSApplication.SharedApplication.KeyWindow;
                if (keyWindow?.ContentView != null)
                {
                    picker.ShowRelativeToRect(
                        keyWindow.ContentView.Bounds,
                        keyWindow.ContentView,
                        NSRectEdge.MinYEdge);
                }
            });

            return Task.CompletedTask;
        }
    }
#endif

#if ANDROID || IOS
    public class MobileShareService : IShareService
    {
        public async Task ShareTextAsync(string title, string text)
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = title,
                Text = text
            });
        }
    }
#endif

public class DesktopFallbackShareService : IShareService
{
    public async Task ShareTextAsync(string title, string text)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var clipboard = desktop.MainWindow?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync($"{title}\n\n{text}");
            }
        }
    }
}