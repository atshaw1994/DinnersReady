using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace DinnersReady.Models;

public class Recipe
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public List<string> UsedIngredients { get; set; } = [];
    public List<string> AdditionalIngredientsNeeded { get; set; } = [];
    public List<string> Instructions { get; set; } = [];
}
