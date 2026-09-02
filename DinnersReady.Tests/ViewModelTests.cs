using System;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using DinnersReady.Models;
using DinnersReady.Services;
using DinnersReady.ViewModels;
using Moq;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace DinnersReady.Tests;

[Trait("Category", "ViewModels")]
public class ViewModelTests
{
    private readonly Mock<IIngredientStoreService> _mockIngredientStore = new();
    private readonly Mock<IRecipeStoreService> _mockRecipeStore = new();
    private readonly Mock<IShareService> _mockShareService = new();

    private MainServicesContext CreateMockServicesContext()
    {
        return new MainServicesContext(
            _mockIngredientStore.Object,
            _mockRecipeStore.Object,
            null!, // RecipeGeneratorViewModel not needed for store tests
            _mockShareService.Object
        );
    }

    #region IngredientViewModel Tests

    [Fact]
    public void IngredientViewModel_UnitDisplay_FormatsCorrectly()
    {
        // Arrange
        var modelGrams = new Ingredient { Unit = "g" };
        var modelLiters = new Ingredient { Unit = "l" };
        var modelEmpty = new Ingredient { Unit = "" };

        // Act
        var vmGrams = new IngredientViewModel(modelGrams);
        var vmLiters = new IngredientViewModel(modelLiters);
        var vmEmpty = new IngredientViewModel(modelEmpty);

        // Assert
        Assert.Equal("g", vmGrams.UnitDisplay);
        Assert.Equal("L", vmLiters.UnitDisplay);
        Assert.Equal("g", vmEmpty.UnitDisplay);
    }

    [Fact]
    public void IngredientViewModel_RequestDelete_InvokesCallback()
    {
        // Arrange
        bool callbackInvoked = false;
        var vm = new IngredientViewModel(
            new Ingredient(),
            onDeleteRequested: deletedVm =>
            {
                callbackInvoked = true;
                return Task.CompletedTask;
            });

        // Act
        vm.RequestDeleteCommand.Execute(null);

        // Assert
        Assert.True(callbackInvoked);
    }

    [Fact]
    public void IngredientViewModel_AcceptEdits_TogglesIsEditing()
    {
        // Arrange
        var vm = new IngredientViewModel(new Ingredient()) { IsEditing = true };

        // Act
        vm.AcceptEditsCommand.Execute(null);

        // Assert
        Assert.False(vm.IsEditing);
    }

    #endregion

    #region RecipeViewModel Tests

    [Theory]
    [InlineData(0, 0, "N/A")]
    [InlineData(45, 0, "45 mins")]
    [InlineData(60, 0, "1 hr")]
    [InlineData(15, 60, "1 hr 15 mins")]
    public void RecipeViewModel_TimeDisplays_FormatCorrectly(int prepMin, int cookMin, string expectedTotalDisplay)
    {
        // Arrange
        var model = new Recipe
        {
            PrepTimeMinutes = prepMin,
            CookTimeMinutes = cookMin
        };

        // Act
        var vm = new RecipeViewModel(model);

        // Assert
        Assert.Equal(expectedTotalDisplay, vm.TotalTimeDisplay);
    }

    [Fact]
    public void RecipeViewModel_ToShareableText_OutputsFormattedString()
    {
        // Arrange
        var recipe = new Recipe
        {
            Title = "Scrambled Eggs",
            PrepTimeMinutes = 2,
            CookTimeMinutes = 3,
            UsedIngredients = ["Eggs", "Butter"],
            Instructions = ["1. Whisk eggs.", "2. Cook in pan."]
        };
        var vm = new RecipeViewModel(recipe);

        // Act
        string shareableText = vm.ToShareableText();

        // Assert
        Assert.Contains("Scrambled Eggs", shareableText);
        Assert.Contains("• Eggs", shareableText);
        Assert.Contains("1. Whisk eggs.", shareableText);
    }

    [Fact]
    public async Task RecipeViewModel_RequestShare_InvokesCallback()
    {
        // Arrange
        bool callbackInvoked = false;
        var vm = new RecipeViewModel(
            new Recipe(),
            onShareRequested: sharedVm =>
            {
                callbackInvoked = true;
                return Task.CompletedTask;
            });

        // Act
        await vm.RequestShareCommand.ExecuteAsync(null);

        // Assert
        Assert.True(callbackInvoked);
    }

    #endregion

    #region MainViewModel Tests

    [Fact]
    public void MainViewModel_OpenAndCloseAddForm_UpdatesIsAddingItem()
    {
        // Arrange
        var mainVm = new MainViewModel(CreateMockServicesContext());

        // Act & Assert - Open Form
        mainVm.OpenAddFormCommand.Execute(null);
        Assert.True(mainVm.IsAddingItem);

        // Act & Assert - Close Form
        mainVm.CloseAddFormCommand.Execute(null);
        Assert.False(mainVm.IsAddingItem);
    }

    [Fact]
    public async Task MainViewModel_RemoveIngredientVmAsync_RemovesFromCollectionAndStore()
    {
        // Arrange
        _mockIngredientStore
            .Setup(s => s.RemoveIngredientAsync(It.IsAny<Ingredient>()))
            .Returns(Task.CompletedTask);

        var mainVm = new MainViewModel(CreateMockServicesContext());
        var itemToDelete = new IngredientViewModel(new Ingredient { Id = "test-1", Name = "Salt", Location = StorageLocation.Pantry });

        mainVm.PantryItems.Add(itemToDelete);

        // Act
        await mainVm.RemoveIngredientVmAsync(itemToDelete);

        // Assert
        Assert.DoesNotContain(itemToDelete, mainVm.PantryItems);
        _mockIngredientStore.Verify(s => s.RemoveIngredientAsync(itemToDelete.Model), Times.Once);
    }

    [Fact]
    public async Task MainViewModel_DeleteRecipeVmAsync_RemovesFromSavedRecipesAndStore()
    {
        // Arrange
        _mockRecipeStore
            .Setup(s => s.RemoveRecipeAsync(It.IsAny<Recipe>()))
            .Returns(Task.CompletedTask);

        var mainVm = new MainViewModel(CreateMockServicesContext());
        var recipeToDelete = new RecipeViewModel(new Recipe { Id = "recipe-1", Title = "Pasta" });

        mainVm.SavedRecipes.Add(recipeToDelete);

        // Act
        await mainVm.DeleteRecipeVmAsync(recipeToDelete);

        // Assert
        Assert.DoesNotContain(recipeToDelete, mainVm.SavedRecipes);
        _mockRecipeStore.Verify(s => s.RemoveRecipeAsync(recipeToDelete.Model), Times.Once);
    }

    #endregion
}