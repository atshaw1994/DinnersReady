using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DinnersReady.Models;
using DinnersReady.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace DinnersReady.ViewModels;

public partial class RecipeGeneratorViewModel(RecipeGeneratorService recipeService, IngredientStore inventoryService) : ObservableObject
{
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
            System.Console.WriteLine($"[RecipeGenerator] HTTP Status: {ex.Status}");
            System.Console.WriteLine($"[RecipeGenerator] Response Body: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[RecipeGenerator] General Exception: {ex.Message}");
        }
        finally
        {
            IsGenerating = false;
        }
    }
}
