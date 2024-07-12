using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

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


    public ObjectPropertyEditorViewModel(IDialogService dialogService)
    {
        this.dialogService = dialogService;
        this.WhenAnyValue(model => model.OwnerObject)
            .Subscribe(owner =>
            {
                EditorFields.Clear();
                if (owner != null)
                {
                    EditorFields.AddRange(owner
                                          .GetType()
                                          .GetProperties()
                                          .Where(FieldAcceptFilter)
                                          .Select(p => new EditorField(p, p.GetValue(owner), GetPossibleValues(p)))
                                          .Where(editorField => editorField.Value is string or bool or int or float || editorField.PossibleValues != null));
                }
            })
            .DisposeWith(Disposables);
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
        Close();
    }

    public bool CanSave() => !HasErrors;

    private static AvaloniaList<object> GetPossibleValues(PropertyInfo propertyInfo)
    {
        return propertyInfo.PropertyType.IsEnum ? new AvaloniaList<object>(propertyInfo.PropertyType.GetFields(BindingFlags.Static | BindingFlags.Public).Select(f => f.GetValue(propertyInfo.PropertyType))) : null;
    }
}
