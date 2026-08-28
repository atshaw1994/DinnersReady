using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DinnersReady.Views
{
    public partial class AiRecipeGenerator : UserControl
    {
        public AiRecipeGenerator()
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine($"AiRecipeGenerator DataContext: {DataContext?.GetType().Name ?? "NULL"}");
        }

        private void DirectButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"AiRecipeGenerator DataContext: {DataContext?.GetType().Name ?? "NULL"}");
        }
    }
}