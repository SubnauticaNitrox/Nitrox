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
    public static readonly AttachedProperty<string> TextProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, string>("Text");

    public static readonly AttachedProperty<string> SubtextProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, string>("Subtext");
    public static readonly AttachedProperty<object> FocusProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, object>("Focus");

    static NitroxAttached()
    {
    }

    public static void SetText(AvaloniaObject element, string value)
    {
        element.SetValue(TextProperty, value);
    }

    public static string GetText(AvaloniaObject element)
    {
        return element.GetValue(TextProperty);
    }

    public static void SetSubtext(AvaloniaObject element, string value)
    {
        element.SetValue(SubtextProperty, value);
    }

    public static string GetSubtext(AvaloniaObject element)
    {
        return element.GetValue(SubtextProperty);
    }

    public static object GetFocus(IAvaloniaObject obj)
    {
        return obj.GetValue(FocusProperty);
    }

    /// <summary>
    ///     Sets the focus to this control when view is loaded.
    /// </summary>
    public static void SetFocus(IAvaloniaObject obj, object value)
    {
        if (obj is Button button)
        {
            async void TryFocusButton()
            {
                int retries = 200;
                do
                {
                    button.Focus();
                    await Task.Delay(10);
                } while (!button.IsFocused && retries-- > 0);
            }

            Dispatcher.UIThread.Post(TryFocusButton);
        }
        else if (obj is IInputElement input)
        {
            Dispatcher.UIThread.Post(() => FocusManager.Instance?.Focus(input, NavigationMethod.Directional));
        }
        obj.SetValue(FocusProperty, value);
    }
}
