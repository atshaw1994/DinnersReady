using Avalonia.Controls;
using Avalonia.Input;
using DinnersReady.Models;
using System;
using System.Diagnostics;

namespace DinnersReady.Views.Mobile;

public partial class IngredientControl : UserControl
{
    public IngredientControl()
    {
        InitializeComponent();
        MainBorder.AddHandler(SwipeGestureEndedEvent, OnSwipeGestureEnded);
    }

    private void OnSwipeGestureEnded(object? sender, SwipeGestureEndedEventArgs e)
    {
        if (DataContext is not Ingredient item) return;

        // Left swipe opens (-X velocity), Right swipe closes (+X velocity)
        if (e.Velocity.X < -100 && item.IsSlidLeft)
        {
            item.IsSlidLeft = false;
        }
        else if (e.Velocity.X > 100 && !item.IsSlidLeft)
        {
            item.IsSlidLeft = true;
        }
    }
}