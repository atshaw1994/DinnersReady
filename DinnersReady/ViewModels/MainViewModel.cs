using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DinnersReady.Models;
using DinnersReady.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DinnersReady.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // Cache JsonSerializerOptions instance to improve performance and satisfy CA1869
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = DinnersReadyJsonContext.Default
    };

    private readonly IngredientStore? _ingredientService;

    // Full library loaded from JSON
    public List<Ingredient> IngredientLibrary { get; private set; } = [];

    #region Properties
    [ObservableProperty] public partial ObservableCollection<Ingredient> PantryItems { get; set; } = [];

    [ObservableProperty] public partial ObservableCollection<Ingredient> FridgeItems { get; set; } = [];

    [ObservableProperty] public partial ObservableCollection<string> IngredientSuggestions { get; set; } = [];

    [ObservableProperty] public partial string NewItemName { get; set; } = string.Empty;

    [ObservableProperty] public partial string NewItemCategory { get; set; } = string.Empty;

    [ObservableProperty] public partial int LocationIndex { get; set; } = 0;

    [ObservableProperty] public partial string SelectedUnit { get; set; } = "g";

    [ObservableProperty] public partial double NewItemQuantity { get; set; } = 1.0;

    [ObservableProperty] public partial DateTimeOffset? NewItemExpiry { get; set; } = DateTimeOffset.Now.AddDays(7);

    [ObservableProperty] public partial bool IsAddingItem { get; set; } = false;

    [ObservableProperty] public partial Ingredient? ItemCurrentlyEditing { get; set; } = null;
    #endregion

    #region Commands
    [RelayCommand]
    private void OpenAddForm() => IsAddingItem = true;

    [RelayCommand]
    private void CloseAddForm() => IsAddingItem = false;

    [RelayCommand]
    private async Task SaveItem()
    {
        var libraryMatch = IngredientLibrary.FirstOrDefault(i => i.Name.Equals(NewItemName, StringComparison.OrdinalIgnoreCase));

        var newItem = new Ingredient
        {
            Id = libraryMatch?.Id ?? NewItemName.Trim().ToLowerInvariant().Replace(" ", "-"),
            Name = NewItemName,
            Category = NewItemCategory,
            Quantity = NewItemQuantity,
            Unit = SelectedUnit,
            ExpiryDate = NewItemExpiry,
            TypicalShelfLifeDays = libraryMatch?.TypicalShelfLifeDays ?? 0,
            Location = (StorageLocation)LocationIndex
        };

        await _ingredientService!.AddIngredientAsync(newItem);

        await LoadInventoryAsync();
        IsAddingItem = false;
    }

    [RelayCommand]
    private void EditItem(Ingredient item)
    {
        if (item == null) return;

        ItemCurrentlyEditing = item;
        NewItemName = item.Name;
        NewItemCategory = item.Category;
        LocationIndex = (int)item.Location;
        SelectedUnit = item.Unit;
        NewItemQuantity = item.Quantity;
        NewItemExpiry = item.ExpiryDate;

        IsAddingItem = true; // Opens the slide-in overlay form
    }

    [RelayCommand]
    private async Task DeleteItemAsync(Ingredient item)
    {
        if (item == null) return;

        await _ingredientService!.RemoveIngredientAsync(item);
        await LoadInventoryAsync();
    }
    #endregion

    // Design-time constructor
    public MainViewModel()
    {
        _ingredientService = null;

        PantryItems =
        [
            new Ingredient { Id = "cumin-ground", Name = "Ground Cumin", Category = "Spices", Quantity = 50, Unit = "g" },
            new Ingredient { Id = "jasmine-rice", Name = "Jasmine Rice", Category = "Grains", Quantity = 1000, Unit = "g" },
            new Ingredient { Id = "olive-oil", Name = "Olive Oil", Category = "Oils", Quantity = 500, Unit = "ml" }
        ];

            FridgeItems =
        [
            new Ingredient { Id = "whole-milk", Name = "Whole Milk", Category = "Dairy", Quantity = 1, Unit = "l" },
            new Ingredient { Id = "cheddar-cheese", Name = "Cheddar Cheese", Category = "Dairy", Quantity = 250, Unit = "g" },
            new Ingredient { Id = "large-eggs", Name = "Large Eggs", Category = "Dairy", Quantity = 12, Unit = "pcs" }
        ];
    }

    public MainViewModel(IngredientStore ingredientStore)
    {
        _ingredientService = ingredientStore;
        LoadLibrary();
        _ = LoadInventoryAsync();
    }

    private void LoadLibrary()
    {
        var uri = new Uri("avares://DinnersReady/Assets/IngredientsLibrary.json");

        if (AssetLoader.Exists(uri))
        {
            using var stream = AssetLoader.Open(uri);

            IngredientLibrary = JsonSerializer.Deserialize<List<Ingredient>>(stream, _jsonOptions) ?? [];

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

    public async Task LoadInventoryAsync()
    {
        var allItems = await _ingredientService!.GetIngredientsAsync();

        PantryItems = new ObservableCollection<Ingredient>(
            allItems.Where(i => i.Location == StorageLocation.Pantry));

        FridgeItems = new ObservableCollection<Ingredient>(
            allItems.Where(i => i.Location == StorageLocation.Fridge));
    }

}