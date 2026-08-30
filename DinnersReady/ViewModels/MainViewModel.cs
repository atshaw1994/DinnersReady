using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DinnersReady.Models;
using DinnersReady.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DinnersReady.ViewModels;

public record MainServicesContext(
    IngredientStore IngredientStore,
    RecipeStore RecipeStore,
    RecipeGeneratorViewModel RecipeGeneratorViewModel
);

public partial class MainViewModel : ObservableValidator
{
    public MainServicesContext Services { get; }

    #region Fields
    // Cache JsonSerializerOptions instance to improve performance and satisfy CA1869
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = DinnersReadyJsonContext.Default
    };

    // Full library loaded from JSON
    public List<Ingredient> IngredientLibrary { get; private set; } = [];

    public bool CanSaveItem => !HasErrors &&
                               !string.IsNullOrWhiteSpace(NewItemName) &&
                               !string.IsNullOrWhiteSpace(NewItemCategory);

    // Direct getter for cleaner XAML binding
    public RecipeGeneratorViewModel RecipeGenerator => Services.RecipeGeneratorViewModel;
    #endregion

    #region Properties
    [ObservableProperty] public partial ObservableCollection<Ingredient> PantryItems { get; set; } = [];

    [ObservableProperty] public partial ObservableCollection<Ingredient> FridgeItems { get; set; } = [];

    [ObservableProperty] public partial ObservableCollection<string> IngredientSuggestions { get; set; } = [];

    [ObservableProperty]
    [Required(ErrorMessage = "Ingredient name is required")]
    [MinLength(1, ErrorMessage = "Name cannot be empty")]
    [NotifyCanExecuteChangedFor(nameof(SaveItemCommand))]
    public partial string NewItemName { get; set; } = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Category is required")]
    [MinLength(1, ErrorMessage = "Category cannot be empty")]
    [NotifyCanExecuteChangedFor(nameof(SaveItemCommand))]
    public partial string NewItemCategory { get; set; } = string.Empty;

    partial void OnNewItemNameChanged(string value)
    {
        ValidateProperty(value, nameof(NewItemName));

        var match = IngredientLibrary.FirstOrDefault(i => i.Name.Equals(value, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            NewItemCategory = match.Category;
            LocationIndex = match.DefaultLocation.Equals("Fridge", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
            SelectedUnit = match.DefaultUnit;
            NewItemExpiry = match.ExpiryDate;
        }
    }
    partial void OnNewItemCategoryChanged(string value) => ValidateProperty(value, nameof(NewItemCategory));

    [ObservableProperty] public partial int LocationIndex { get; set; } = 0;

    [ObservableProperty] public partial string SelectedUnit { get; set; } = "g";

    [ObservableProperty] public partial double NewItemQuantity { get; set; } = 1.0;

    [ObservableProperty] public partial DateTimeOffset? NewItemExpiry { get; set; } = DateTimeOffset.Now.AddDays(7);

    [ObservableProperty] public partial bool IsAddingItem { get; set; } = false;

    [ObservableProperty] public partial bool IsEditingItem { get; set; } = false;

    [ObservableProperty] public partial Ingredient? ItemCurrentlyEditing { get; set; } = null;
    #endregion

    #region Commands
    [RelayCommand]
    private void OpenAddForm()
    {
        ValidateAllProperties();
        IsAddingItem = true;
    }

    [RelayCommand]
    private void CloseAddForm() => IsAddingItem = false;

    [RelayCommand(CanExecute = nameof(CanSaveItem))]
    private async Task SaveItem()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        var libraryMatch = IngredientLibrary.FirstOrDefault(i => i.Name.Equals(NewItemName, StringComparison.OrdinalIgnoreCase));

        var newItem = new Ingredient
        {
            Id = libraryMatch?.Id ?? NewItemName.Trim().ToLowerInvariant().Replace(" ", "-"),
            Name = NewItemName,
            Category = NewItemCategory,
            Quantity = NewItemQuantity,
            Unit = SelectedUnit,
            ExpiryDate = NewItemExpiry,
            Location = (StorageLocation)LocationIndex
        };

        await Services.IngredientStore!.AddIngredientAsync(newItem);

        await LoadInventoryAsync();
        IsAddingItem = false;
        IsEditingItem = false;
    }

    [RelayCommand]
    private void Mobile_EditItem(Ingredient item)
    {
        if (item == null) return;

        ItemCurrentlyEditing = item;
        NewItemName = item.Name;
        NewItemCategory = item.Category;
        LocationIndex = (int)item.Location;
        SelectedUnit = item.Unit;
        NewItemQuantity = item.Quantity;
        NewItemExpiry = item.ExpiryDate;

        IsAddingItem = true;
        IsEditingItem = true; // Opens the slide-in overlay form
    }

    [RelayCommand]
    public async Task ToggleEditIngredient(Ingredient ingredient)
    {
        if (ingredient is null) return;

        if (ingredient.IsEditing)
        {
            await Services.IngredientStore!.RemoveIngredientAsync(ingredient);
            await Services.IngredientStore!.AddIngredientAsync(ingredient);
            await LoadInventoryAsync();
            ingredient.IsEditing = false;
        }
        else
        {
            ingredient.IsEditing = true;
        }
    }

    [RelayCommand]
    private async Task DeleteItemAsync(Ingredient item)
    {
        if (item == null) return;

        await Services.IngredientStore!.RemoveIngredientAsync(item);
        await LoadInventoryAsync();
    }
    #endregion

    // Design-time constructor
    public MainViewModel()
    {
        if (Design.IsDesignMode)
        {
            Services = new MainServicesContext(
                null!,
                null!,
                new RecipeGeneratorViewModel()
            );

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
        else
        {
            Services = null!;
        }
    }

    public MainViewModel(MainServicesContext services)
    {
        Services = services;

        // Initialize collections so bindings don't fail null checks
        IngredientLibrary = [];
        IngredientSuggestions = [];
        PantryItems = [];
        FridgeItems = [];

        // Re-evaluate SaveItem command whenever validation errors change
        ErrorsChanged += (s, e) => SaveItemCommand.NotifyCanExecuteChanged();

        // Defer all loading until the WASM UI thread finishes its initial layout pass
        Dispatcher.UIThread.Post(async () => await InitializeAsync(), DispatcherPriority.Background);
    }

    private async Task InitializeAsync()
    {
        await LoadLibraryAsync();
        await LoadInventoryAsync();
    }

    private async Task LoadLibraryAsync()
    {
        try
        {
            var uri = new Uri("avares://DinnersReady/Assets/IngredientsLibrary.json");
            Stream? stream = null;

            // Prefer AssetLoader without calling Exists() first
            if (AssetLoader.Exists(uri))
            {
                stream = AssetLoader.Open(uri);
            }
            else
            {
                var assembly = typeof(MainViewModel).Assembly;
                stream = assembly.GetManifestResourceStream("DinnersReady.Assets.IngredientsLibrary.json");
            }

            if (stream != null)
            {
                using (stream)
                {
                    // Use Async deserialization to keep WASM responsive
                    var items = await JsonSerializer.DeserializeAsync<List<Ingredient>>(stream, _jsonOptions);

                    if (items != null)
                    {
                        IngredientLibrary = items;

                        IngredientSuggestions.Clear();
                        foreach (var name in IngredientLibrary.Select(i => i.Name))
                        {
                            IngredientSuggestions.Add(name);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading ingredient library: {ex.Message}");
        }
    }

    public async Task LoadInventoryAsync()
    {
        var allItems = await Services.IngredientStore.GetIngredientsAsync();

        // Clear and repopulate on the UI thread safely
        PantryItems.Clear();
        foreach (var item in allItems.Where(i => i.Location == StorageLocation.Pantry))
            PantryItems.Add(item);

        FridgeItems.Clear();
        foreach (var item in allItems.Where(i => i.Location == StorageLocation.Fridge))
            FridgeItems.Add(item);
    }
}