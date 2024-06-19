using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Nitrox.Launcher.UI.Controls;

/// <summary>
/// A basic Rich Textbox. Supports bold, italic and underline tags.
/// </summary>
/// <remarks>
/// Tag legend:<br/>
///  [b][/b] - Bold <br/>
///  [i][/i] - Italicize <br/>
///  [u][/u] - Underline <br/>
/// </remarks>
/// <example>
/// [b]Text[/b] => <b>Text</b> <br/>
/// [i]Text[/i] => <i>Text</i> <br/>
/// [u]Text[/u] => <u>Text</u> <br/>
/// </example>
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
    
    private void ParseTextAndAddInlines(string text)
    {
        // Define the tags and their corresponding formatting actions.
        Dictionary<string, Action<Run>> tagActions = new()
        {
            { "b", run => run.FontWeight = FontWeight.Bold },
            { "i", run => run.FontStyle = FontStyle.Italic },
            { "u", run => run.TextDecorations = [new() { Location = TextDecorationLocation.Underline }] }
        };

        // Split the text into sections based on the tags.
        List<(List<string> Tags, string Text)> sections = new();
        List<string> currentTags = new();
        StringBuilder currentText = new();

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '[')
            {
                // Start of a tag.
                sections.Add((new List<string>(currentTags), currentText.ToString()));
                currentText.Clear();

                int tagEndIndex = text.IndexOf(']', i);
                if (tagEndIndex == -1)
                {
                    // Invalid tag, ignore it.
                    continue;
                }

                string tag = text.Substring(i + 1, tagEndIndex - i - 1);
                if (tag.StartsWith('/'))
                {
                    currentTags.Remove(tag.Substring(1));
                }
                else
                {
                    currentTags.Add(tag);
                }

                i = tagEndIndex;
            }
            else
            {
                // Regular text.
                currentText.Append(text[i]);
            }
        }

        // Add the last section.
        sections.Add((currentTags, currentText.ToString()));

        // Apply the formatting to each section based on its tag.
        foreach ((List<string> Tags, string Text) section in sections)
        {
            Run run = new(section.Text);
            
            if (string.IsNullOrEmpty(run.Text) || run.Text == "Empty") continue;
            
            foreach (string tag in section.Tags)
            {
                if (tagActions.TryGetValue(tag, out Action<Run> action))
                {
                    action(run);
                }
            }
            Inlines.Add(run);
        }
    }

}