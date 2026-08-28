using Avalonia.Controls;
using System.Runtime.Versioning;

namespace DinnersReady.Views.FullSize;

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("macos")]
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}