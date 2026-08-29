using System.Collections.Generic;

namespace DinnersReady.Models;

public class GeneratedRecipe
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PrepTimeMinutes { get; set; }
    public string PrepTimeDisplay => FormatTime(PrepTimeMinutes);
    public int CookTimeMinutes { get; set; }
    public string CookTimeDisplay => FormatTime(CookTimeMinutes);
    public string TotalTimeDisplay => FormatTime(PrepTimeMinutes + CookTimeMinutes);
    public List<string> UsedIngredients { get; set; } = [];
    public List<string> AdditionalIngredientsNeeded { get; set; } = [];
    public List<string> Instructions { get; set; } = [];

    private static string FormatTime(int totalMinutes)
    {
        if (totalMinutes <= 0)
            return "N/A";

        int hours = totalMinutes / 60;
        int mins = totalMinutes % 60;

        if (hours == 0)
            return $"{mins} min{(mins == 1 ? "" : "s")}";

        if (mins == 0)
            return $"{hours} hr{(hours == 1 ? "" : "s")}";

        return $"{hours} hr{(hours == 1 ? "" : "s")} {mins} min{(mins == 1 ? "" : "s")}";
    }
}
