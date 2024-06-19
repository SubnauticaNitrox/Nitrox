using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Nitrox.Launcher.UI.Controls;

public class RichTextBlock : TextBlock
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty)
        {
            Inlines?.Clear();
            ParseTextAndAddInlines(Text ?? "");
        }
    }
    
    // TODO: Fix code to allow for multiple tags to be applied to the same text.
    private void ParseTextAndAddInlines(string text)
    {
        // Define the tags and their corresponding formatting actions.
        Dictionary<string, Action<Run>> tagActions = new()
        {
            { "<u>", run => run.TextDecorations = run.TextDecorations = [new() { Location = TextDecorationLocation.Underline }] },
            { "<b>", run => run.FontWeight = FontWeight.Bold },
            { "<i>", run => run.FontStyle = FontStyle.Italic }
        };

        // Split the text into sections based on the tags.
        List<(string Tag, string Text)> sections = new();
        string currentTag = "";
        StringBuilder currentText = new();
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '<')
            {
                // Start of a tag.
                sections.Add((currentTag, currentText.ToString()));
                currentText.Clear();

                int tagEndIndex = text.IndexOf('>', i);
                if (tagEndIndex == -1)
                {
                    // Invalid tag, ignore it.
                    continue;
                }

                currentTag = text.Substring(i, tagEndIndex - i + 1);
                i = tagEndIndex;
            }
            else
            {
                // Regular text.
                currentText.Append(text[i]);
            }
        }

        // Add the last section.
        sections.Add((currentTag, currentText.ToString()));

        // Apply the formatting to each section based on its tag.
        foreach ((string Tag, string Text) section in sections)
        {
            if (tagActions.TryGetValue(section.Tag, out Action<Run> action))
            {
                // The section has a tag, apply the corresponding formatting.
                Run run = new Run(section.Text);
                action(run);
                Inlines.Add(run);
            }
            else
            {
                // The section doesn't have a tag, add it as regular text.
                Inlines.Add(new Run(section.Text));
            }
        }
    }

}