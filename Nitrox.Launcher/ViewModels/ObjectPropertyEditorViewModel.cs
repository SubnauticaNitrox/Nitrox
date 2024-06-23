using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Text;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Server;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class ObjectPropertyEditorViewModel : ModalViewModelBase
{
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

    public ObjectPropertyEditorViewModel()
    {
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
                                          .Select(p => new EditorField(p, p.GetValue(owner), GetPossibleValues(p))));
                    // TEMP in order to see values:
                    StringBuilder consoleText = new();
                    foreach (EditorField editorField in EditorFields)
                    {
                        consoleText.AppendLine($"{editorField.PropertyInfo.Name} = {editorField.Value} ({editorField.Value.GetType()})");
                        if (editorField.PossibleValues.Any())
                        {
                            consoleText.AppendLine($"  Possible values: {string.Join(", ", editorField.PossibleValues)}");
                        }
                    }
                    EditorFields.Add(new EditorField(typeof(string).GetProperty("TEMP: Values"), consoleText.ToString(), []));
                }
            })
            .DisposeWith(Disposables);
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    public void Save()
    {
        foreach (EditorField field in EditorFields)
        {
            try
            {
                field.PropertyInfo.SetValue(OwnerObject, field.Value);
            }
            catch (ArgumentException)
            {
                // ignored
            }
        }
        DialogResult = true;
        Close();
    }

    public bool CanSave() => !HasErrors;
    
    private AvaloniaList<object> GetPossibleValues(PropertyInfo propertyInfo)
    {
        //return new AvaloniaList<object>([NitroxGameMode.CREATIVE, NitroxGameMode.SURVIVAL, NitroxGameMode.HARDCORE]); // For testing
        if (propertyInfo.PropertyType.IsEnum)
        {
            return new AvaloniaList<object>(Enum.GetValues(propertyInfo.PropertyType));
        }
        return [];
    }
}
