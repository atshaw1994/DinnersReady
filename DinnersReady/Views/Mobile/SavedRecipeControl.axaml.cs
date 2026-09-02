using Avalonia.Controls;
using Avalonia.Input;
using DinnersReady.ViewModels;

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
            if (DataContext is not RecipeViewModel vm || vm.Model is null) return;

            // Left swipe opens (-X velocity), Right swipe closes (+X velocity)
            if (e.Velocity.X > -100)
            {
                if (vm.IsSlidRight)
                {
                    vm.IsSlidRight = false;
                    vm.IsSlidLeft = false;
                }
                else if (!vm.IsSlidLeft)
                {
                    vm.IsSlidLeft = true;
                    vm.IsSlidRight = false;
                }
            }
            else if (e.Velocity.X < 100)
            {
                if (vm.IsSlidLeft)
                {
                    vm.IsSlidRight = false;
                    vm.IsSlidLeft = false;
                }
                else if (!vm.IsSlidRight)
                {
                    vm.IsSlidRight = true;
                    vm.IsSlidLeft = false;
                }
            }
        }
    }
}