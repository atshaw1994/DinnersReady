using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DinnersReady.Models;
using System;
using System.Threading.Tasks;

namespace DinnersReady.ViewModels;

public partial class IngredientViewModel(
    Ingredient model,
    Func<IngredientViewModel, Task>? onDeleteRequested = null,
    Func<Ingredient, Task>? onEditRequested = null,
    Action<Ingredient>? onOverlayRequested = null) : ObservableObject
{
    public IngredientViewModel() : this(new Ingredient()) { }

    public Func<IngredientViewModel, Task>? OnDeleteRequested { get; } = onDeleteRequested;
    public Func<Ingredient, Task>? OnEditRequested { get; } = onEditRequested;
    public Action<Ingredient>? OnOverlayRequested { get; } = onOverlayRequested;

    public Ingredient Model { get; set; } = model ?? throw new ArgumentNullException(nameof(model));

    #region Wrapped Model Properties

    public string Id
    {
        get => Model.Id;
        set => SetProperty(Model.Id, value, Model, (m, val) => m.Id = val);
    }

    public string Name
    {
        get => Model.Name;
        set => SetProperty(Model.Name, value, Model, (m, val) => m.Name = val);
    }

    public string Category
    {
        get => Model.Category;
        set => SetProperty(Model.Category, value, Model, (m, val) => m.Category = val);
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
        set => SetProperty(Model.Location, value, Model, (m, val) => m.Location = val);
    }

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

    [ObservableProperty]
    public partial bool IsSlidLeft { get; set; }

    [ObservableProperty]
    public partial bool IsSlidRight { get; set; }

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

    #endregion
}