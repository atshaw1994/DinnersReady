namespace DinnersReady.Models;

public class Ingredient
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DefaultLocation { get; set; } = "Pantry";
    public string DefaultUnit { get; set; } = "g";
    public int TypicalShelfLifeDays { get; set; }
}
