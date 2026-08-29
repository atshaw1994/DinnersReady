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

public partial class RecipeGeneratorViewModel(
    RecipeGeneratorService recipeService, 
    IngredientStore inventoryService, 
    RecipeStore recipeStore,
    IShareService shareService) : ObservableObject
{
    // Parameterless constructor strictly for Avalonia Previewer / Design Time
    public RecipeGeneratorViewModel() : this(null!, null!, null!, null!) { }

    [ObservableProperty] public partial bool IsGenerating { get; set; } = false;

    [ObservableProperty] public partial GeneratedRecipe? CurrentRecipe { get; set; } = null;

    [RelayCommand]
    public async Task GenerateRecipeAsync(CancellationToken ct)
    {
        IsGenerating = true;
        try
        {
            var ingredients = await inventoryService.GetIngredientsAsync();
            var ingredientNames = ingredients.Select(i => i.Name);
            CurrentRecipe = await recipeService.GenerateRecipeAsync(ingredientNames, ct);
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
            await recipeStore.AddRecipeAsync(CurrentRecipe);
    }

    [RelayCommand]
    public async Task ShareRecipe()
    {
        if (CurrentRecipe is null || shareService is null) return;

        var sb = new StringBuilder();
        sb.AppendLine(CurrentRecipe.Title);
        sb.AppendLine($"Prep: {CurrentRecipe.PrepTimeDisplay} | Cook: {CurrentRecipe.CookTimeDisplay}");
        sb.AppendLine();

        if (CurrentRecipe.UsedIngredients?.Count > 0)
        {
            sb.AppendLine("Ingredients:");
            foreach (var ing in CurrentRecipe.UsedIngredients)
            {
                sb.AppendLine($"• {ing}");
            }
            sb.AppendLine();
        }

        if (CurrentRecipe.AdditionalIngredientsNeeded?.Count > 0)
        {
            sb.AppendLine("Additional Ingredients Needed:");
            foreach (var ing in CurrentRecipe.AdditionalIngredientsNeeded)
            {
                sb.AppendLine($"• {ing}");
            }
            sb.AppendLine();
        }

        if (CurrentRecipe.Instructions?.Count > 0)
        {
            sb.AppendLine("Instructions:");
            foreach (var step in CurrentRecipe.Instructions)
            {
                sb.AppendLine(step);
            }
        }

        await shareService.ShareTextAsync(CurrentRecipe.Title ?? "Recipe", sb.ToString());
    }

    [RelayCommand]
    public void ClearRecipe() => CurrentRecipe = null;
}
