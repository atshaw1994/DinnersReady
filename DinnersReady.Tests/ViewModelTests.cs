using Avalonia.Headless.XUnit;
using DinnersReady.Models;
using DinnersReady.Services;
using DinnersReady.ViewModels;
using Moq;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace DinnersReady.Tests;

[Trait("Category", "ViewModels")]
public class ViewModelTests
{
    private static MainViewModel CreateMainViewModel(
        out Mock<IIngredientStoreRepository> ingredientRepoMock,
        out Mock<IRecipeStoreRepository> recipeRepoMock,
        out Mock<IShareService> shareServiceMock)
    {
        ingredientRepoMock = new Mock<IIngredientStoreRepository>();
        recipeRepoMock = new Mock<IRecipeStoreRepository>();
        shareServiceMock = new Mock<IShareService>();

        ingredientRepoMock
            .Setup(s => s.LoadAllAsync())
            .ReturnsAsync(new List<Ingredient>());

        recipeRepoMock
            .Setup(s => s.LoadAllAsync())
            .ReturnsAsync(new List<GeneratedRecipe>());

        var ingredientStore = new IngredientStore(ingredientRepoMock.Object);
        var recipeStore = new RecipeStore(recipeRepoMock.Object);

        var recipeGeneratorViewModel = new RecipeGeneratorViewModel(
            new RecipeGeneratorContext(null!, ingredientStore, recipeStore, shareServiceMock.Object));

        var context = new MainServicesContext(
            ingredientStore,
            recipeStore,
            recipeGeneratorViewModel,
            shareServiceMock.Object);

        return new MainViewModel(context);
    }

    [Fact]
    public void NewItemName_Empty_CanSaveItem_IsFalse()
    {
        var vm = CreateMainViewModel(out _, out _, out _);

        vm.NewItemName = string.Empty;

        Assert.False(vm.CanSaveItem);
    }

    [Fact]
    public void NewItemName_And_Category_Set_CanSaveItem_IsTrue()
    {
        var vm = CreateMainViewModel(out _, out _, out _);

        vm.NewItemName = "Chicken Breast";
        vm.NewItemCategory = "Meat";

        Assert.True(vm.CanSaveItem);
    }

    [Fact]
    public async Task SaveItemCommand_ValidItem_CallsRepositorySaveAsync()
    {
        var vm = CreateMainViewModel(out var ingredientRepoMock, out _, out _);

        vm.NewItemName = "Chicken Breast";
        vm.NewItemCategory = "Meat";
        vm.NewItemQuantity = 2;

        await vm.SaveItemCommand.ExecuteAsync(null);

        ingredientRepoMock.Verify(
            s => s.SaveAsync(It.Is<Ingredient>(i => i.Name == "Chicken Breast" && i.Category == "Meat")),
            Times.Once);
    }

    [Fact]
    public async Task SaveItemCommand_InvalidItem_DoesNotCallRepositorySaveAsync()
    {
        var vm = CreateMainViewModel(out var ingredientRepoMock, out _, out _);

        vm.NewItemName = string.Empty;
        vm.NewItemCategory = string.Empty;

        await vm.SaveItemCommand.ExecuteAsync(null);

        ingredientRepoMock.Verify(s => s.SaveAsync(It.IsAny<Ingredient>()), Times.Never);
    }

    [Fact]
    public async Task LoadInventoryAsync_PopulatesPantryAndFridgeCollections()
    {
        var vm = CreateMainViewModel(out var ingredientRepoMock, out _, out _);

        ingredientRepoMock
            .Setup(s => s.LoadAllAsync())
            .ReturnsAsync(new List<Ingredient>
            {
                new() { Id = "pantry-item", Name = "Rice", Location = StorageLocation.Pantry },
                new() { Id = "fridge-item", Name = "Milk", Location = StorageLocation.Fridge }
            });

        //await vm.LoadInventoryAsync();

        Assert.Single(vm.PantryItems);
        Assert.Equal("Rice", vm.PantryItems.Single().Name);
        Assert.Single(vm.FridgeItems);
        Assert.Equal("Milk", vm.FridgeItems.Single().Name);
    }

    [Fact]
    public async Task DeleteIngredientAsyncCommand_CallsRepositoryDeleteAsync()
    {
        var vm = CreateMainViewModel(out var ingredientRepoMock, out _, out _);
        var item = new Ingredient { Id = "test-item", Name = "Test" };

        //await vm.DeleteIngredientCommand.ExecuteAsync(item);

        ingredientRepoMock.Verify(s => s.DeleteAsync(item), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipeAsyncCommand_RemovesRecipeFromSavedRecipesAndStore()
    {
        var vm = CreateMainViewModel(out _, out var recipeRepoMock, out _);
        var recipe = new GeneratedRecipe { Id = "recipe-1", Title = "Test Recipe" };
        vm.SavedRecipes.Add(recipe);

        await vm.DeleteRecipeCommand.ExecuteAsync(recipe);

        recipeRepoMock.Verify(s => s.DeleteAsync(recipe), Times.Once);
        Assert.DoesNotContain(recipe, vm.SavedRecipes);
    }

    [Fact]
    public void ClearRecipeCommand_ResetsCurrentRecipeToNull()
    {
        var vm = new RecipeGeneratorViewModel(new RecipeGeneratorContext(null!, null!, null!, null!))
        {
            CurrentRecipe = new GeneratedRecipe { Id = "r1", Title = "Something" }
        };

        vm.ClearRecipeCommand.Execute(null);

        Assert.Null(vm.CurrentRecipe);
    }

    [Fact]
    public async Task SaveRecipeCommand_WithCurrentRecipe_CallsRepositorySaveAsync()
    {
        var recipeRepoMock = new Mock<IRecipeStoreRepository>();
        recipeRepoMock.Setup(r => r.LoadAllAsync()).ReturnsAsync(new List<GeneratedRecipe>());

        var recipeStore = new RecipeStore(recipeRepoMock.Object);
        var vm = new RecipeGeneratorViewModel(new RecipeGeneratorContext(null!, null!, recipeStore, null!))
        {
            CurrentRecipe = new GeneratedRecipe { Id = "r1", Title = "Something" }
        };

        await vm.SaveRecipeCommand.ExecuteAsync(CancellationToken.None);

        recipeRepoMock.Verify(s => s.SaveAsync(vm.CurrentRecipe), Times.Once);
    }

    [Fact]
    public async Task SaveRecipeCommand_WithoutCurrentRecipe_DoesNotThrow()
    {
        var vm = new RecipeGeneratorViewModel(new RecipeGeneratorContext(null!, null!, null!, null!));

        var exception = await Record.ExceptionAsync(() => vm.SaveRecipeCommand.ExecuteAsync(CancellationToken.None));

        Assert.Null(exception);
    }
}