using System;
using Avalonia;
using Avalonia.Controls;

namespace Nitrox.Launcher.Models.Controls;

/// <summary>
///     A basic Rich Textbox with support for text selection. Supports bold, italic, underline, colors and hyperlinks.
/// </summary>
/// <remarks>
///     Tag legend:<br />
///     [b][/b] - Bold <br />
///     [i][/i] - Italicize <br />
///     [u][/u] - Underline <br />
///     [#colorHex][/#colorHex] - Change text color <br />
///     [Flavor text](example.com) <br />
/// </remarks>
/// <example>
///     [b]Text[/b] => <b>Text</b> <br />
///     [i]Text[/i] => <i>Text</i> <br />
///     [u]Text[/u] => <u>Text</u> <br />
///     [#0000FF]Text[/#0000FF] => Text (with blue foreground) <br />
///     <a href="https://example.com">Flavor text</a> <br />
/// </example>
public class SelectableRichTextBlock : SelectableTextBlock
{

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty)
        {
            Inlines?.Clear();
            Inlines = RichTextBlock.ParseTextAndAddInlines(Text ?? "", Inlines);
            // If all text was just tags, set Text to empty. Otherwise, it will be displayed as fallback by Avalonia.
            if (Inlines?.Count < 1)
            {
                Text = "";
            }
        }
    }

    protected override Type StyleKeyOverride { get; } = typeof(SelectableTextBlock);
}
