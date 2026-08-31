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
        if (e.Velocity.X > -100)
        {
            if (item.IsSlidRight)
            {
                item.IsSlidRight = false;
                item.IsSlidLeft = false;
            }
            else if (!item.IsSlidLeft)
            {
                item.IsSlidLeft = true;
                item.IsSlidRight = false;
            }
        }
        else if (e.Velocity.X < 100)
        {
            if (item.IsSlidLeft)
            {
                item.IsSlidRight = false;
                item.IsSlidLeft = false;
            }
            else if (!item.IsSlidRight)
            {
                item.IsSlidRight = true;
                item.IsSlidLeft = false;
            }
        }
    }
}