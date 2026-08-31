using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace DinnersReady.Models;

public enum StorageLocation
{
    Fridge,
    Pantry
}

public partial class Ingredient : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DefaultLocation { get; set; } = "Pantry";
    public string DefaultUnit { get; set; } = "g";
    public double Quantity { get; set; } = 0.0;
    public string Unit { get; set; } = "g";
    public string UnitDisplay
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Unit)) return "g";

            return Unit.Equals("l", StringComparison.OrdinalIgnoreCase)
                ? "L"
                : Unit.ToLowerInvariant();
        }
    }

    public DateTimeOffset? ExpiryDate { get; set; } = DateTimeOffset.Now.AddDays(7);
    public StorageLocation Location { get; set; } = StorageLocation.Fridge;

    [ObservableProperty] public partial bool IsEditing { get; set; } = false;
    [ObservableProperty] public partial bool IsSlidLeft { get; set; } = false;
    [ObservableProperty] public partial bool IsSlidRight { get; set; } = false;
}
