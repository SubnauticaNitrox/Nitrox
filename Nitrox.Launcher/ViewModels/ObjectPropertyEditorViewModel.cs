using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.ViewModels;

public partial class ObjectPropertyEditorViewModel : ModalViewModelBase
{
    private readonly IDialogService dialogService;

    [ObservableProperty]
    private AvaloniaList<EditorField> editorFields = [];

    [ObservableProperty]
    private object ownerObject;

    private string title;

    public string Title
    {
        get => title ?? $"{OwnerObject.GetType().Name} editor";
        set => title = value;
    }

    /// <summary>
    ///     Gets or sets the field filter to use. If filter returns false, it will omit the field.
    /// </summary>
    public Func<PropertyInfo, bool> FieldAcceptFilter { get; set; } = _ => true;

    public ObjectPropertyEditorViewModel() : this(null)
    {
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OwnerObject))
        {
            EditorFields.Clear();
            EditorFields.AddRange(OwnerObject
                                  .GetType()
                                  .GetProperties()
                                  .Where(FieldAcceptFilter)
                                  .Select(p => new EditorField(p, p.GetValue(OwnerObject), GetPossibleValues(p)))
                                  .Where(editorField => editorField.Value is string or bool or int or float || editorField.PossibleValues != null));
        }
        base.OnPropertyChanged(e);
    }

    public ObjectPropertyEditorViewModel(IDialogService dialogService)
    {
        this.dialogService = dialogService;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task Save()
    {
        foreach (EditorField field in EditorFields)
        {
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

    private static AvaloniaList<object> GetPossibleValues(PropertyInfo propertyInfo) =>
        propertyInfo.PropertyType.IsEnum ? new AvaloniaList<object>(propertyInfo.PropertyType.GetFields(BindingFlags.Static | BindingFlags.Public).Select(f => f.GetValue(propertyInfo.PropertyType))) : null;
}
