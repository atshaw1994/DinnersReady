using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using DinnersReady.Models;

namespace DinnersReady.Views.Mobile
{
    public partial class SavedRecipeControl : UserControl
    {
        public SavedRecipeControl()
        {
            InitializeComponent();
            MainBorder.AddHandler(SwipeGestureEndedEvent, OnSwipeGestureEnded);
        }

        private void OnSwipeGestureEnded(object? sender, SwipeGestureEndedEventArgs e)
        {
            if (DataContext is not GeneratedRecipe item) return;

            // Left swipe opens (-X velocity), Right swipe closes (+X velocity)
            if (e.Velocity.X < -100 && !item.IsSlidRight)
            {
                item.IsSlidRight = true;
            }
            else if (e.Velocity.X > 100 && item.IsSlidRight)
            {
                item.IsSlidRight = false;
            }
        }
    }
}