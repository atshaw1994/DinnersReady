using Avalonia.Controls;
using System.Runtime.Versioning;

namespace DinnersReady.Views.Mobile;

[SupportedOSPlatform("android")]
[SupportedOSPlatform("ios")]
public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }
}