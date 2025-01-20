using System;
using Avalonia;
using Avalonia.Controls;

namespace Nitrox.Launcher.Models.Controls;

/// <inheritdoc cref="RichTextBlock"/>
public class SelectableRichTextBlock : SelectableTextBlock
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty)
        {
            Inlines?.Clear();
            RichTextBlock.ParseTextAndAddInlines(Text ?? "", Inlines);
            // If all text was just tags, set Text to empty. Otherwise, it will be displayed as fallback by Avalonia.
            if (Inlines?.Count < 1)
            {
                Text = "";
            }
        }
    }

    protected override Type StyleKeyOverride { get; } = typeof(SelectableTextBlock);
}
