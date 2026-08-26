using Avalonia.Controls;
using System;

namespace DinnersReady.Models;

public enum StorageLocation
{
    Fridge,
    Pantry
}

public class Ingredient
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DefaultLocation { get; set; } = "Pantry";
    public string DefaultUnit { get; set; } = "g";
    public int TypicalShelfLifeDays { get; set; }
    public double Quantity { get; set; } = 0.0;
    public string Unit { get; set; } = "g";
    public string UnitDisplay
    {
        get
        {
            if (Unit == null) return "g";
            if (Unit.Equals("l", StringComparison.OrdinalIgnoreCase)) return "L";
            return "g";
        }
    }

    public DateTimeOffset? ExpiryDate { get; set; } = DateTimeOffset.Now.AddDays(7);
    public StorageLocation Location { get; set; } = StorageLocation.Fridge;
}
