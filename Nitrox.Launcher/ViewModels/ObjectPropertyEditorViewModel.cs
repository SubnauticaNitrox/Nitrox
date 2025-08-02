using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.ViewModels;

internal partial class ObjectPropertyEditorViewModel(DialogService dialogService) : ModalViewModelBase
{
    private readonly DialogService dialogService = dialogService;

    [ObservableProperty]
    private AvaloniaList<EditorField> editorFields = [];

    [ObservableProperty]
    private object? ownerObject;

    [ObservableProperty]
    private string? title;

    /// <summary>
    ///     Gets or sets the field filter to use. If filter returns false, it will omit the field.
    /// </summary>
    public Func<PropertyInfo, bool> FieldAcceptFilter { get; set; } = _ => true;

    [ObservableProperty]
    private bool disableButtons;

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task Save()
    {
        foreach (EditorField field in EditorFields)
        {
            if (field.Value is null && field.PropertyInfo.PropertyType.IsValueType)
            {
                continue;
            }

            try
            {
                field.PropertyInfo.SetValue(OwnerObject, Convert.ChangeType(field.Value, field.PropertyInfo.PropertyType));
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync(ex, description: field.ToString());
            }
        }
        Close(ButtonOptions.Ok);
    }

    public bool CanSave() => !HasErrors;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OwnerObject))
        {
            EditorFields.Clear();
            if (OwnerObject != null)
            {
                EditorFields.AddRange(OwnerObject
                                      .GetType()
                                      .GetProperties()
                                      .Where(FieldAcceptFilter)
                                      .Where(p => p.CanWrite)
                                      .Select(p => new EditorField(p, p.GetValue(OwnerObject), GetPossibleValues(p)))
                                      .Where(editorField => editorField.Value is string or bool or int or float || editorField.PossibleValues is [..]));
            }
        }
        base.OnPropertyChanged(e);
    }

    private static AvaloniaList<object> GetPossibleValues(PropertyInfo propertyInfo) =>
        propertyInfo.PropertyType.IsEnum ? new AvaloniaList<object>(propertyInfo.PropertyType.GetFields(BindingFlags.Static | BindingFlags.Public).Select(f => f.GetValue(propertyInfo.PropertyType))) : [];
}
