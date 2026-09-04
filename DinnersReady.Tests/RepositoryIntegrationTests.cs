using DinnersReady.Models;
using DinnersReady.Services;

namespace DinnersReady.Tests;

[Trait("Category", "Integration")]
public class RepositoryIntegrationTests : IDisposable
{
    private readonly string _testStorageDir;

    public RepositoryIntegrationTests()
    {
        // Create a unique temporary directory for each test run
        _testStorageDir = Path.Combine(Path.GetTempPath(), "DinnersReady_Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testStorageDir);
    }

    public void Dispose()
    {
        // Cleanup temp directory after test completes
        if (Directory.Exists(_testStorageDir))
        {
            try
            {
                Directory.Delete(_testStorageDir, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors on temp files if held briefly by OS
            }
        }
    }

    #region IngredientStoreRepository Tests

    [Fact]
    public async Task IngredientStoreRepository_SaveAndLoadAll_PersistsToDisk()
    {
        // Arrange
        var repo = new IngredientStoreRepository(new FileSystemStorageProvider());
        var ingredient = new Ingredient
        {
            Id = "ing-int-1",
            Name = "Organic Milk",
            Quantity = 1.5,
            Unit = "L",
            Location = StorageLocation.Fridge
        };

        // Act
        await repo.SaveAsync(ingredient);
        var loadedItems = (await repo.LoadAllAsync()).ToList();

        // Assert
        Assert.Contains(loadedItems, i => i.Id == ingredient.Id && i.Name == "Organic Milk");

        // Clean up store item
        await repo.DeleteAsync(ingredient);
    }

    [Fact]
    public async Task IngredientStoreRepository_Delete_RemovesFileFromDisk()
    {
        // Arrange
        var repo = new IngredientStoreRepository(new FileSystemStorageProvider());
        var ingredient = new Ingredient { Id = "ing-int-2", Name = "Butter" };

        await repo.SaveAsync(ingredient);

        // Act
        await repo.DeleteAsync(ingredient);
        var loadedItems = await repo.LoadAllAsync();

        // Assert
        Assert.DoesNotContain(loadedItems, i => i.Id == ingredient.Id);
    }

    #endregion

    #region RecipeStoreRepository Tests

    [Fact]
    public async Task RecipeStoreRepository_SaveAndLoadAll_PersistsJsonCorrectly()
    {
        // Arrange
        var repo = new RecipeStoreRepository(new FileSystemStorageProvider());
        var recipe = new Recipe
        {
            Id = "rec-int-1",
            Title = "Homemade Pizza",
            PrepTimeMinutes = 20,
            CookTimeMinutes = 15,
            UsedIngredients = ["Flour", "Yeast", "Cheese"],
            Instructions = ["Mix dough", "Bake at 450F"]
        };

        // Act
        await repo.SaveAsync(recipe);
        var loadedRecipes = (await repo.LoadAllAsync()).ToList();

        // Assert
        var savedRecipe = loadedRecipes.FirstOrDefault(r => r.Id == recipe.Id);
        Assert.NotNull(savedRecipe);
        Assert.Equal("Homemade Pizza", savedRecipe.Title);
        Assert.Equal(3, savedRecipe.UsedIngredients.Count);

        // Clean up store item
        await repo.DeleteAsync(recipe.Id);
    }

    #endregion

    [Fact]
    public async Task Repository_LoadAll_HandlesCorruptedJsonGracefully()
    {
        // Arrange
        var repo = new IngredientStoreRepository(new FileSystemStorageProvider());
        var validIngredient = new Ingredient { Id = "valid-1", Name = "Salt" };
        await repo.SaveAsync(validIngredient);

        // Write a corrupted file directly into the storage folder
        string corruptFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DinnersReady", "Storage", "corrupt_data.json"
        );
        await File.WriteAllTextAsync(corruptFilePath, "{ invalid json content }", TestContext.Current.CancellationToken);

        // Act
        var loadedItems = (await repo.LoadAllAsync()).ToList();

        // Assert
        Assert.Contains(loadedItems, i => i.Id == validIngredient.Id);

        // Clean up
        await repo.DeleteAsync(validIngredient);
        if (File.Exists(corruptFilePath)) File.Delete(corruptFilePath);
    }

    [Fact]
    public async Task Repository_ClearAllAsync_RemovesAllStoredFiles()
    {
        // Arrange
        var repo = new RecipeStoreRepository(new FileSystemStorageProvider());
        await repo.SaveAsync(new Recipe { Id = "clear-1", Title = "Recipe 1" });
        await repo.SaveAsync(new Recipe { Id = "clear-2", Title = "Recipe 2" });

        // Act
        await repo.ClearAllAsync();
        var loaded = await repo.LoadAllAsync();

        // Assert
        Assert.Empty(loaded);
    }
}