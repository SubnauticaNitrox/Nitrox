using Avalonia;
using Avalonia.Interactivity;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
///     Container class for our attached properties.
/// </summary>
public class NitroxAttached : AvaloniaObject
{
    static NitroxAttached()
    {
    }

    public static readonly AttachedProperty<string> TextProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, string>("Text");
    
    public static readonly AttachedProperty<string> SubtextProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, string>("Subtext");

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
}
