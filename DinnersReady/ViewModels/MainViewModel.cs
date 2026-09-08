using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
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

public record MainServicesContext(
    IIngredientStoreService IngredientStore,
    IRecipeStoreService RecipeStore,
    RecipeGeneratorViewModel RecipeGeneratorViewModel,
    IShareService ShareService
);

public partial class MainViewModel : ObservableValidator
{
    public MainServicesContext Services { get; }

    // Design-time constructor
    public MainViewModel()
    {
        if (Design.IsDesignMode)
        {
            Services = new MainServicesContext(
                null!,
                null!,
                new RecipeGeneratorViewModel(),
                null!
            );

            PantryItems =
            [
                new IngredientViewModel(new Ingredient { Id = "cumin-ground", Name = "Ground Cumin", Category = "Spices", Quantity = 50, Unit = "g" }),
                new IngredientViewModel(new Ingredient { Id = "jasmine-rice", Name = "Jasmine Rice", Category = "Grains", Quantity = 1000, Unit = "g" }),
                new IngredientViewModel(new Ingredient { Id = "olive-oil", Name = "Olive Oil", Category = "Oils", Quantity = 500, Unit = "ml" })
            ];

            FridgeItems =
            [
                new IngredientViewModel(new Ingredient { Id = "whole-milk", Name = "Whole Milk", Category = "Dairy", Quantity = 1, Unit = "l" }),
                new IngredientViewModel(new Ingredient { Id = "cheddar-cheese", Name = "Cheddar Cheese", Category = "Dairy", Quantity = 250, Unit = "g" }),
                new IngredientViewModel(new Ingredient { Id = "large-eggs", Name = "Large Eggs", Category = "Dairy", Quantity = 12, Unit = "pcs" })
            ];

            SavedRecipes =
            [
                new RecipeViewModel(DesignData.Recipes.CheesyOmelette, null!, null!)
            ];

            FormItem = new IngredientViewModel
            {
                IsAddingItem = true
            };
        }
        else
        {
            Services = null!;
            FormItem = new IngredientViewModel();
        }
    }

    public MainViewModel(MainServicesContext services)
    {
        Services = services;

        IngredientLibrary = [];
        IngredientSuggestions = [];
        PantryItems = [];
        FridgeItems = [];

        FormItem = new IngredientViewModel(new Ingredient(), onSaveRequested: SaveFormItemAsync, onLibraryLookupRequested: LookupLibraryItem);

        Dispatcher.UIThread.Post(async () => await InitializeAsync(), DispatcherPriority.Background);
    }

    #region Fields & Getters

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = DinnersReadyJsonContext.Default
    };

    public List<Ingredient> IngredientLibrary { get; private set; } = [];

    public RecipeGeneratorViewModel RecipeGenerator => Services.RecipeGeneratorViewModel;

    public IngredientViewModel FormItem { get; }

    private Ingredient? LookupLibraryItem(string name) =>
        IngredientLibrary.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    #endregion

    #region Properties

    [ObservableProperty] public partial ObservableCollection<IngredientViewModel> PantryItems { get; set; } = [];
    [ObservableProperty] public partial ObservableCollection<IngredientViewModel> FridgeItems { get; set; } = [];
    [ObservableProperty] public partial ObservableCollection<RecipeViewModel> SavedRecipes { get; set; } = [];
    [ObservableProperty] public partial ObservableCollection<string> IngredientSuggestions { get; set; } = [];

    #endregion

    #region Ingredient Form Handling

    private async Task SaveFormItemAsync(Ingredient item)
    {
        await Services.IngredientStore.AddIngredientAsync(item);
        await LoadInventoryAsync();
    }

    #endregion

    #region Initialization Methods

    private async Task InitializeAsync()
    {
        await LoadLibraryAsync();
        await LoadInventoryAsync();
        await LoadRecipesAsync();
    }

    private async Task LoadLibraryAsync()
    {
        try
        {
            var uri = new Uri("avares://DinnersReady/Assets/IngredientsLibrary.json");
            Stream? stream = AssetLoader.Exists(uri)
                ? AssetLoader.Open(uri)
                : typeof(MainViewModel).Assembly.GetManifestResourceStream("DinnersReady.Assets.IngredientsLibrary.json");

            if (stream != null)
            {
                using (stream)
                {
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

    private async Task LoadInventoryAsync()
    {
        var allItems = await Services.IngredientStore.GetIngredientsAsync();

        PantryItems.Clear();
        foreach (var item in allItems.Where(i => i.Location == StorageLocation.Pantry))
        {
            PantryItems.Add(new IngredientViewModel(
                item, 
                onSaveRequested: SaveFormItemAsync, 
                onLibraryLookupRequested: LookupLibraryItem, 
                onDeleteRequested: 
                RemoveIngredientVmAsync
            ));
        }

        FridgeItems.Clear();
        foreach (var item in allItems.Where(i => i.Location == StorageLocation.Fridge))
        {
            FridgeItems.Add(new IngredientViewModel(
                item, 
                onSaveRequested: SaveFormItemAsync, 
                onLibraryLookupRequested: LookupLibraryItem, 
                onDeleteRequested: RemoveIngredientVmAsync
            ));
        }
    }

    private async Task LoadRecipesAsync()
    {
        var allItems = await Services.RecipeStore.GetRecipesAsync();

        SavedRecipes.Clear();
        foreach (var item in allItems)
        {
            SavedRecipes.Add(new RecipeViewModel(item, DeleteRecipeVmAsync, Services.ShareService));
        }
    }

    #endregion

    #region Ingredient & Recipe Item Delegate Handlers

    public async Task RemoveIngredientVmAsync(IngredientViewModel vm)
    {
        if (vm == null) return;

        bool removedFromPantry = PantryItems?.Remove(vm) ?? false;
        bool removedFromFridge = FridgeItems?.Remove(vm) ?? false;

        if ((removedFromPantry || removedFromFridge) && Services?.IngredientStore != null)
        {
            await Services.IngredientStore.RemoveIngredientAsync(vm.Model);
        }
    }

    public async Task EditIngredientAsync(Ingredient ingredient) =>
        await Services.IngredientStore.ModifyIngredientAsync(ingredient);

    public void ShowOverlay(Ingredient ingredient)
    {
        if (ingredient == null) return;
        FormItem.LoadForEdit(ingredient);
    }

    public async Task DeleteRecipeVmAsync(RecipeViewModel vm)
    {
        if (vm == null) return;

        if (SavedRecipes?.Remove(vm) == true && Services?.RecipeStore != null)
        {
            await Services.RecipeStore.RemoveRecipeAsync(vm.Model);
        }
    }

    #endregion
}