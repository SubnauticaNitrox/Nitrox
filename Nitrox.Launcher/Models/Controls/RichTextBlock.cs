using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Nitrox.Launcher.Models.Controls;

/// <summary>
///     A basic Rich Textbox. Supports bold, italic, underline and hyperlinks.
/// </summary>
/// <remarks>
///     Tag legend:<br />
///     [b][/b] - Bold <br />
///     [i][/i] - Italicize <br />
///     [u][/u] - Underline <br />
///     [Flavor text](example.com) <br />
/// </remarks>
/// <example>
///     [b]Text[/b] => <b>Text</b> <br />
///     [i]Text[/i] => <i>Text</i> <br />
///     [u]Text[/u] => <u>Text</u> <br />
///     <a href="https://example.com">Flavor text</a> <br />
/// </example>
public partial class RichTextBlock : TextBlock
{
    private static readonly TextDecorationCollection underlineTextDecoration = [new() { Location = TextDecorationLocation.Underline }];

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty)
        {
            Inlines?.Clear();
            ParseTextAndAddInlines(Text ?? "");
        }
    }

    [GeneratedRegex(@"\[\/?([^]]+)\](?:\(([^\)]*)\))?")]
    private static partial Regex TagParserRegex { get; }

    private void ParseTextAndAddInlines(ReadOnlySpan<char> text)
    {
        if (Inlines == null)
        {
            return;
        }
        Regex.ValueMatchEnumerator matchEnumerator = TagParserRegex.EnumerateMatches(text);
        if (!matchEnumerator.MoveNext())
        {
            Inlines.Add(new Run(text.ToString()));
            return;
        }

        ValueMatch lastRange = default;
        Dictionary<string, Action<Run>> activeTags = new(3);
        do
        {
            ValueMatch range = matchEnumerator.Current;

            // Handle text in-between previous and current tag.
            ReadOnlySpan<char> textPart = text[(lastRange.Index + lastRange.Length)..range.Index];
            if (!textPart.IsEmpty)
            {
                Inlines.Add(CreateRunWithTags(textPart.ToString(), activeTags));
            }

            // Handle current tag (this tracks state of active tags at current text position)
            ReadOnlySpan<char> match = text.Slice(range.Index, range.Length);
            switch (match)
            {
                case ['[', '/', ..]:
                    activeTags.Remove(match[2..^1].ToString());
                    break;
                case "[b]":
                    activeTags["b"] = static run => run.FontWeight = FontWeight.Bold;
                    break;
                case "[u]":
                    activeTags["u"] = static run => run.TextDecorations = underlineTextDecoration;
                    break;
                case "[i]":
                    activeTags["i"] = static run => run.FontStyle = FontStyle.Italic;
                    break;
                case ['[', ..] when match.IndexOf("](", StringComparison.OrdinalIgnoreCase) > -1:
                    TextBlock textBlock = new();
                    textBlock.Classes.Add("link");
                    textBlock.Text = match[1..match.IndexOfAny("]")].ToString();
                    textBlock.Tag = match[(match.IndexOfAny("(")+1)..match.IndexOfAny(")")].ToString();
                    Inlines.Add(textBlock);
                    break;
                default:
                    // Unknown tag, let's handle as normal text (issue is likely due to input text not knowing about this RichTextBox format)
                    Inlines.Add(CreateRunWithTags(match.ToString(), activeTags));
                    break;
            }

            lastRange = range;
        } while (matchEnumerator.MoveNext());

        // Handle any final text (after the last tag).
        ReadOnlySpan<char> lastPart = text[(lastRange.Index + lastRange.Length)..];
        if (!lastPart.IsEmpty)
        {
            Inlines.Add(CreateRunWithTags(lastPart.ToString(), activeTags));
        }
    }

    private Run CreateRunWithTags(string text, Dictionary<string, Action<Run>> tags)
    {
        Run run = new(text);
        foreach (KeyValuePair<string, Action<Run>> tag in tags)
        {
            tag.Value(run);
        }
        return run;
    }

    protected override Type StyleKeyOverride { get; } = typeof(TextBlock);
}
