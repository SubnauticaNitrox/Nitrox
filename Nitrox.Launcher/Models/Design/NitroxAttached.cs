using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
///     Container class for our attached properties.
/// </summary>
public class NitroxAttached : AvaloniaObject
{
    public static readonly AttachedProperty<bool> SelectedProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, bool>("Selected");

    static NitroxAttached()
    {
    }
    
    /// <summary>
    ///     Sets the focus to this control when view is loaded.
    /// </summary>
    public static void SetFocus(AvaloniaObject obj, object value)
    {
        static void VisualOnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            (sender as IInputElement)?.Focus();
        }

        switch (obj)
        {
            case Visual visual when visual is IInputElement:
                Dispatcher.UIThread.Post(() => (visual as IInputElement)?.Focus());
                visual.AttachedToVisualTree += VisualOnAttachedToVisualTree;
                break;
            default:
                throw new NotSupportedException($@"Element {obj} must be an {nameof(IInputElement)} to support ""{nameof(SetFocus)}""");
        }
    }

    public static bool GetSelected(AvaloniaObject element) => element.GetValue(SelectedProperty);

    public static void SetSelected(AvaloniaObject obj, bool value) => obj.SetValue(SelectedProperty, value);
}
