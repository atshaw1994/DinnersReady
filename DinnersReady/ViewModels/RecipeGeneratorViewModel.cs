using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DinnersReady.Models;
using DinnersReady.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DinnersReady.ViewModels;

public record RecipeGeneratorContext(
    RecipeGeneratorService RecipeService,
    IngredientStore IngredientService,
    RecipeStore RecipeStore,
    IShareService ShareService,
    Action<Recipe>? OnShareRequested = null,
    Action<Recipe>? OnDeleteRequested = null
);

public partial class RecipeGeneratorViewModel : ObservableObject
{
    public RecipeGeneratorContext Services { get; }

    // Parameterless constructor strictly for Avalonia Previewer / Design Time
    public RecipeGeneratorViewModel()
    {
        Services = new RecipeGeneratorContext(
            null!,
            null!,
            null!,
            null!
        );
    }

    public RecipeGeneratorViewModel(RecipeGeneratorContext services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    [ObservableProperty] public partial bool IsGenerating { get; set; } = false;

    [ObservableProperty] public partial Recipe? CurrentRecipe { get; set; } = null;

    [RelayCommand]
    public async Task GenerateRecipeAsync(CancellationToken ct)
    {
        IsGenerating = true;
        try
        {
            var ingredients = await Services.IngredientService.GetIngredientsAsync();
            var ingredientNames = ingredients.Select(i => i.Name);
            CurrentRecipe = await Services.RecipeService.GenerateRecipeAsync(ingredientNames, ct);
        }
        catch (System.ClientModel.ClientResultException ex)
        {
            Console.WriteLine($"[RecipeGenerator] HTTP Status: {ex.Status}");
            Console.WriteLine($"[RecipeGenerator] Response Body: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RecipeGenerator] General Exception: {ex.Message}");
        }
        finally
        {
            IsGenerating = false;
        }
    }

    [RelayCommand]
    public async Task RegenerateRecipeAsync(CancellationToken ct) => await GenerateRecipeAsync(ct);

    [RelayCommand]
    public async Task SaveRecipe(CancellationToken ct) 
    {
        if (CurrentRecipe is not null)
            await Services.RecipeStore.AddRecipeAsync(CurrentRecipe);
    }

    [RelayCommand]
    public async Task RequestShare() => Services.OnShareRequested?.Invoke(CurrentRecipe!);

    [RelayCommand]
    public void ClearRecipe() => CurrentRecipe = null;
}
