using Avalonia.Controls;
using System.Runtime.Versioning;

namespace DinnersReady.Views.FullSize;

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("macos")]
[SupportedOSPlatform("browser")]
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}