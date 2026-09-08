using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DinnersReady.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DinnersReady.ViewModels;

public partial class IngredientViewModel(
    Ingredient model,
    Func<IngredientViewModel, Task>? onDeleteRequested = null,
    Func<Ingredient, Task>? onEditRequested = null,
    Action<Ingredient>? onOverlayRequested = null,
    Func<Ingredient, Task>? onSaveRequested = null,
    Func<string, Ingredient?>? onLibraryLookupRequested = null) : ObservableValidator
{
    public IngredientViewModel() : this(new Ingredient()) { }

    public Func<IngredientViewModel, Task>? OnDeleteRequested { get; } = onDeleteRequested;
    public Func<Ingredient, Task>? OnEditRequested { get; } = onEditRequested;
    public Action<Ingredient>? OnOverlayRequested { get; } = onOverlayRequested;
    public Func<Ingredient, Task>? OnSaveRequested { get; } = onSaveRequested;
    public Func<string, Ingredient?>? OnLibraryLookupRequested { get; } = onLibraryLookupRequested;

    public Ingredient Model { get; set; } = model ?? throw new ArgumentNullException(nameof(model));

    #region Wrapped Model Properties

    public string Id
    {
        get => Model.Id;
        set => SetProperty(Model.Id, value, Model, (m, val) => m.Id = val);
    }

    [Required(ErrorMessage = "Ingredient name is required")]
    [MinLength(1, ErrorMessage = "Name cannot be empty")]
    public string Name
    {
        get => Model.Name;
        set
        {
            if (SetProperty(Model.Name, value, Model, (m, val) => m.Name = val))
            {
                ValidateProperty(value, nameof(Name));
                SaveItemCommand.NotifyCanExecuteChanged();
                ApplyLibraryMatch(value);
            }
        }
    }

    [Required(ErrorMessage = "Category is required")]
    [MinLength(1, ErrorMessage = "Category cannot be empty")]
    public string Category
    {
        get => Model.Category;
        set
        {
            if (SetProperty(Model.Category, value, Model, (m, val) => m.Category = val))
            {
                ValidateProperty(value, nameof(Category));
                SaveItemCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public double Quantity
    {
        get => Model.Quantity;
        set => SetProperty(Model.Quantity, value, Model, (m, val) => m.Quantity = val);
    }

    public string Unit
    {
        get => Model.Unit;
        set
        {
            if (SetProperty(Model.Unit, value, Model, (m, val) => m.Unit = val))
            {
                OnPropertyChanged(nameof(UnitDisplay));
            }
        }
    }

    public string UnitDisplay => string.IsNullOrWhiteSpace(Unit)
        ? "g"
        : Unit.Equals("l", StringComparison.OrdinalIgnoreCase) ? "L" : Unit.ToLowerInvariant();

    public DateTimeOffset? ExpiryDate
    {
        get => Model.ExpiryDate;
        set => SetProperty(Model.ExpiryDate, value, Model, (m, val) => m.ExpiryDate = val);
    }

    public StorageLocation Location
    {
        get => Model.Location;
        set
        {
            if (SetProperty(Model.Location, value, Model, (m, val) => m.Location = val))
            {
                OnPropertyChanged(nameof(LocationIndex));
            }
        }
    }

    public int LocationIndex
    {
        get => (int)Location;
        set => Location = (StorageLocation)value;
    }

    #endregion

    #region Add/Edit Form Helpers

    private void ApplyLibraryMatch(string name)
    {
        var match = OnLibraryLookupRequested?.Invoke(name);
        if (match != null)
        {
            Category = match.Category;
            Location = match.DefaultLocation.Equals("Fridge", StringComparison.OrdinalIgnoreCase) ? StorageLocation.Fridge : StorageLocation.Pantry;
            Unit = match.DefaultUnit;
            ExpiryDate = match.ExpiryDate;
        }
    }

    public bool CanSaveItem => !HasErrors &&
                                !string.IsNullOrWhiteSpace(Name) &&
                                !string.IsNullOrWhiteSpace(Category);

    #endregion

    #region States

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    partial void OnIsEditingChanged(bool oldValue, bool newValue)
    {
        if (oldValue && !newValue)
        {
            OnEditRequested?.Invoke(Model);
        }
    }

    [ObservableProperty] public partial bool IsSlidLeft { get; set; }

    [ObservableProperty] public partial bool IsSlidRight { get; set; }

    [ObservableProperty] public partial bool IsAddingItem { get; set; }

    [ObservableProperty] public partial bool IsEditingItem { get; set; }

    #endregion

    #region Commands

    [RelayCommand]
    public void RequestDelete() => _ = OnDeleteRequested?.Invoke(this);

    [RelayCommand]
    public void RequestOverlay() => OnOverlayRequested?.Invoke(Model);

    [RelayCommand]
    public void RequestEdit() => IsEditing = true;

    [RelayCommand]
    public void Edit() => IsEditing = true;

    [RelayCommand]
    public void AcceptEdits() => IsEditing = false;

    public void LoadForEdit(Ingredient ingredient)
    {
        ArgumentNullException.ThrowIfNull(ingredient);
        Model = new Ingredient
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            Category = ingredient.Category,
            DefaultLocation = ingredient.DefaultLocation,
            DefaultUnit = ingredient.DefaultUnit,
            Quantity = ingredient.Quantity,
            Unit = ingredient.Unit,
            ExpiryDate = ingredient.ExpiryDate,
            Location = ingredient.Location
        };
        OnPropertyChanged((string?)null);
        ValidateAllProperties();
        IsEditingItem = true;
        IsAddingItem = true;
    }

    [RelayCommand]
    public void OpenAddForm()
    {
        Model = new Ingredient();
        OnPropertyChanged((string?)null);
        ValidateAllProperties();
        IsEditingItem = false;
        IsAddingItem = true;
    }

    [RelayCommand]
    public void CloseAddForm()
    {
        IsAddingItem = false;
        IsEditingItem = false;
    }

    [RelayCommand(CanExecute = nameof(CanSaveItem))]
    public async Task SaveItem()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        if (string.IsNullOrWhiteSpace(Model.Id))
        {
            Model.Id = Name.Trim().ToLowerInvariant().Replace(" ", "-");
        }

        if (OnSaveRequested != null)
        {
            await OnSaveRequested.Invoke(Model);
        }

        IsAddingItem = false;
        IsEditingItem = false;
    }

    #endregion
}