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
    public enum ThemeOption
    {
        DARK,
        LIGHT
    }

    public static readonly AttachedProperty<string> TextProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, string>("Text");
    public static readonly AttachedProperty<string> SubtextProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, string>("Subtext");
    public static readonly AttachedProperty<object> FocusProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, object>("Focus");
    public static readonly AttachedProperty<bool> SelectedProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, bool>("Selected");
    public static readonly AttachedProperty<ThemeOption> ThemeProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, ThemeOption>("Theme", inherits: true, defaultValue: ThemeOption.DARK);

    static NitroxAttached()
    {
    }

    public static void SetText(AvaloniaObject element, string value)
    {
        if (element is not Button button || !button.Classes.Contains("nitrox"))
        {
            throw new NotSupportedException($@"Button must have class ""nitrox"" to support attached property ""{nameof(TextProperty)}"".");
        }
        element.SetValue(TextProperty, value);
    }

    public static string GetText(AvaloniaObject element) => element.GetValue(TextProperty);

    public static void SetSubtext(AvaloniaObject element, string value)
    {
        if (element is not Button button || !button.Classes.Contains("nitrox"))
        {
            throw new NotSupportedException($@"Button must have class ""nitrox"" to support attached property ""{nameof(SubtextProperty)}"".");
        }
        element.SetValue(SubtextProperty, value);
    }

    public static string GetSubtext(AvaloniaObject element) => element.GetValue(SubtextProperty);

    public static object GetFocus(IAvaloniaObject obj) => obj.GetValue(FocusProperty);

    /// <summary>
    ///     Sets the focus to this control when view is loaded.
    /// </summary>
    public static void SetFocus(IAvaloniaObject obj, object value)
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

        if (obj is Button button)
        {
            Dispatcher.UIThread.Post(() => TryFocusButton(button));
        }
        else if (obj is IInputElement input)
        {
            Dispatcher.UIThread.Post(() => FocusManager.Instance?.Focus(input, NavigationMethod.Directional));
        }
        else
        {
            throw new NotSupportedException($@"Element {obj} must be a {nameof(Button)} or {nameof(IInputElement)} to support attached property ""{nameof(FocusProperty)}""");
        }
        obj.SetValue(FocusProperty, value);
    }

    public static bool GetSelected(AvaloniaObject element) => element.GetValue(SelectedProperty);

    public static void SetSelected(IAvaloniaObject obj, bool value) => obj.SetValue(SelectedProperty, value);

    public static ThemeOption GetTheme(IAvaloniaObject avaloniaObject) => avaloniaObject.GetValue(ThemeProperty);

    /// <summary>
    ///     Sets the theme of the current visual and its children to the given theme.
    /// </summary>
    public static void SetTheme(IAvaloniaObject avaloniaObject, ThemeOption value) => avaloniaObject.SetValue(ThemeProperty, value);
}
