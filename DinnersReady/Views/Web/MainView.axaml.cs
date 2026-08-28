using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Runtime.Versioning;

namespace DinnersReady.Views.Web;

[SupportedOSPlatform("browser")]
public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }
}