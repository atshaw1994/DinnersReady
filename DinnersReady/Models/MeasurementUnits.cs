using System;
using System.Collections.Generic;
using System.Text;

namespace DinnersReady.Models;

public static class MeasurementUnits
{
    // Volume - Imperial & US Customary
    public const string Teaspoon = "tsp";
    public const string Tablespoon = "tbsp";
    public const string FluidOunce = "fl oz";
    public const string Cup = "cup";
    public const string Pint = "pt";
    public const string Quart = "qt";
    public const string Gallon = "gal";

    // Volume - Metric
    public const string Milliliter = "ml";
    public const string Liter = "l";

    // Weight & Mass - Imperial & US Customary
    public const string Ounce = "oz";
    public const string Pound = "lb";

    // Weight & Mass - Metric
    public const string Milligram = "mg";
    public const string Gram = "g";
    public const string Kilogram = "kg";

    // Informal & Count-Based
    public const string Pinch = "pinch";
    public const string Dash = "dash";
    public const string Drop = "drop";
    public const string Clove = "clove";
    public const string Head = "head";
    public const string Piece = "pc";
    public const string Slice = "slice";
    public const string Can = "can";
    public const string Package = "pkg";
    public const string Stick = "stick"; // e.g., butter
    public const string Bunch = "bunch";
    public const string Stalk = "stalk";
    public const string Sprig = "sprig";

    /// <summary>
    /// Complete list suitable for Avalonia ComboBox or ListBox DataTemplates.
    /// </summary>
    public static readonly List<string> AllUnits = new()
    {
        // Volume (Metric)
        Milliliter, Liter,
        // Volume (US/Imperial)
        Teaspoon, Tablespoon, FluidOunce, Cup, Pint, Quart, Gallon,
        // Weight (Metric)
        Milligram, Gram, Kilogram,
        // Weight (US/Imperial)
        Ounce, Pound,
        // Count / Kitchen Measures
        Pinch, Dash, Drop, Clove, Head, Piece, Slice, Can, Package, Stick, Bunch, Stalk, Sprig
    };
}
