using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DinnersReady.Models;

namespace DinnersReady.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // Cache JsonSerializerOptions instance to improve performance and satisfy CA1869
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    // Full library loaded from JSON
    public List<Ingredient> IngredientLibrary { get; private set; } = [];

    #region Properties
    [ObservableProperty] public partial ObservableCollection<string> IngredientSuggestions { get; set; } = [];

    [ObservableProperty] public partial string NewItemName { get; set; } = string.Empty;

    [ObservableProperty] public partial string NewItemCategory { get; set; } = string.Empty;

    [ObservableProperty] public partial int LocationIndex { get; set; } = 0;

    [ObservableProperty] public partial string SelectedUnit { get; set; } = "g";

    [ObservableProperty] public partial double NewItemQuantity { get; set; } = 1.0;

    [ObservableProperty] public partial DateTimeOffset? NewItemExpiry { get; set; } = DateTimeOffset.Now.AddDays(7);

    [ObservableProperty] public partial bool IsAddingItem { get; set; } = false;
    #endregion

    #region Commands
    [RelayCommand]
    private void OpenAddForm() => IsAddingItem = true;

    [RelayCommand]
    private void CloseAddForm() => IsAddingItem = false;

    [RelayCommand]
    private void SaveItem()
    {
        // Add to active inventory collection here...
        IsAddingItem = false;
    }
    #endregion

    public MainViewModel() => LoadLibrary();

    private void LoadLibrary()
    {
        string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IngredientsLibrary.json");
        if (File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            IngredientLibrary = JsonSerializer.Deserialize<List<Ingredient>>(json, JsonSerializerOptions) ?? [];

            IngredientSuggestions = new ObservableCollection<string>(
                IngredientLibrary.Select(i => i.Name)
            );
        }
    }

    // Auto-fill logic triggered when user picks or types a recognized item
    partial void OnNewItemNameChanged(string value)
    {
        var match = IngredientLibrary.FirstOrDefault(i =>
            i.Name.Equals(value, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            NewItemCategory = match.Category;
            LocationIndex = match.DefaultLocation.Equals("Fridge", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
            SelectedUnit = match.DefaultUnit;

            if (match.TypicalShelfLifeDays > 0)
            {
                NewItemExpiry = DateTimeOffset.Now.AddDays(match.TypicalShelfLifeDays);
            }
        }
    }

}