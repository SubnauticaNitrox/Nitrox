using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
///     Container class for our attached properties.
/// </summary>
public class NitroxAttached : AvaloniaObject
{
    public static readonly AttachedProperty<object> FocusProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, object>("Focus");
    public static readonly AttachedProperty<bool> SelectedProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, bool>("Selected");

    static NitroxAttached()
    {
    }
    
    public static object GetFocus(AvaloniaObject obj) => obj.GetValue(FocusProperty);

    /// <summary>
    ///     Sets the focus to this control when view is loaded.
    /// </summary>
    public static void SetFocus(AvaloniaObject obj, object value)
    {
        static async void TryFocusButton(Button btn)
        {
            int retries = 200;
            do
            {
                btn.Focus();
                await Task.Delay(10);
            } while (!btn.IsFocused && retries-- > 0);
        }

        switch (obj)
        {
            case Button button:
                Dispatcher.UIThread.Post(() => TryFocusButton(button));
                break;
            case IInputElement input:
                Dispatcher.UIThread.Post(() => input.Focus());
                break;
            default:
                throw new NotSupportedException($@"Element {obj} must be a {nameof(Button)} or {nameof(IInputElement)} to support attached property ""{nameof(FocusProperty)}""");
        }
        obj.SetValue(FocusProperty, value);
    }

    public static bool GetSelected(AvaloniaObject element) => element.GetValue(SelectedProperty);

    public static void SetSelected(AvaloniaObject obj, bool value) => obj.SetValue(SelectedProperty, value);
}
