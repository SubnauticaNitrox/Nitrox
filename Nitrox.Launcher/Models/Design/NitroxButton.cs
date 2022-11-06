using Avalonia;
using Avalonia.Controls;

namespace Nitrox.Launcher.Models.Design;

public class NitroxButton : Border
{
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<NitroxButton, string>(nameof(Text));
    public static readonly StyledProperty<string> CaptionProperty = AvaloniaProperty.Register<NitroxButton, string>(nameof(Caption));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Caption
    {
        get => GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public NitroxButton()
    {
        AffectsRender<NitroxButton>(TextProperty);
        AffectsMeasure<Border>(TextProperty);
    }
}
