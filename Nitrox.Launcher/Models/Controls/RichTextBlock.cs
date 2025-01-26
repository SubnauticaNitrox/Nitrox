using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Nitrox.Launcher.Models.Controls;

/// <summary>
///     A basic Rich Textbox. Supports bold, italic, underline, colors and hyperlinks.
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
public partial class RichTextBlock : TextBlock
{
    private static readonly TextDecorationCollection underlineTextDecoration = [new() { Location = TextDecorationLocation.Underline }];

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty)
        {
            Inlines?.Clear();
            ParseTextAndAddInlines(Text ?? "", Inlines);
            // If all text was just tags, set Text to empty. Otherwise, it will be displayed as fallback by Avalonia.
            if (Inlines?.Count < 1)
            {
                Text = "";
            }
        }
    }

    [GeneratedRegex(@"\[\/?([^]]+)\](?:\(([^\)]*)\))?")]
    private static partial Regex TagParserRegex { get; }

    public static void ParseTextAndAddInlines(ReadOnlySpan<char> text, InlineCollection inlines)
    {
        if (inlines == null)
        {
            return;
        }
        Regex.ValueMatchEnumerator matchEnumerator = TagParserRegex.EnumerateMatches(text);
        if (!matchEnumerator.MoveNext())
        {
            inlines.Add(new Run(text.ToString()));
            return;
        }

        ValueMatch lastRange = default;
        Dictionary<string, Action<Run, string>> activeTags = new(4);
        do
        {
            ValueMatch range = matchEnumerator.Current;

            // Handle text in-between previous and current tag.
            ReadOnlySpan<char> textPart = text[(lastRange.Index + lastRange.Length)..range.Index];
            if (!textPart.IsEmpty)
            {
                inlines.Add(CreateRunWithTags(textPart.ToString(), activeTags));
            }

            // Handle current tag (this tracks state of active tags at current text position)
            ReadOnlySpan<char> match = text.Slice(range.Index, range.Length);
            switch (match)
            {
                case ['[', '/', ..]:
                    activeTags.Remove(match[2..^1].ToString());
                    break;
                case "[b]":
                    activeTags["b"] = static (run, _) => run.FontWeight = FontWeight.Bold;
                    break;
                case "[u]":
                    activeTags["u"] = static (run, _) => run.TextDecorations = underlineTextDecoration;
                    break;
                case "[i]":
                    activeTags["i"] = static (run, _) => run.FontStyle = FontStyle.Italic;
                    break;
                case ['[', ..] when match.IndexOf("](", StringComparison.OrdinalIgnoreCase) > -1:
                    TextBlock textBlock = new();
                    textBlock.Classes.Add("link");
                    textBlock.Text = match[1..match.IndexOfAny("]")].ToString();
                    textBlock.Tag = match[(match.IndexOfAny("(")+1)..match.IndexOfAny(")")].ToString();
                    inlines.Add(textBlock);
                    break;
                case ['[', '#', ..]:
                    ReadOnlySpan<char> colorCode = match[1..match.IndexOfAny("]")];
                    if (!Color.TryParse(colorCode, out Color _))
                    {
                        goto default;
                    }
                    activeTags[colorCode.ToString()] = static (run, tag) => run.Foreground = new SolidColorBrush(Color.Parse(tag));
                    break;
                default:
                    // Unknown tag, let's handle as normal text (issue is likely due to input text not knowing about this RichTextBox format)
                    inlines.Add(CreateRunWithTags(match.ToString(), activeTags));
                    break;
            }

            lastRange = range;
        } while (matchEnumerator.MoveNext());

        // Handle any final text (after the last tag).
        ReadOnlySpan<char> lastPart = text[(lastRange.Index + lastRange.Length)..];
        if (!lastPart.IsEmpty)
        {
            inlines.Add(CreateRunWithTags(lastPart.ToString(), activeTags));
        }
    }

    private static Run CreateRunWithTags(string text, Dictionary<string, Action<Run, string>> tags)
    {
        Run run = new(text);
        KeyValuePair<string, Action<Run, string>>? lastColorTag = null;
        foreach (KeyValuePair<string, Action<Run, string>> pair in tags)
        {
            switch (pair.Key)
            {
                case ['#', ..]:
                    // Optimization: only the last color needs to be applied for the current run, ignore all others.
                    lastColorTag = pair;
                    break;
                default:
                    pair.Value(run, pair.Key);
                    break;
            }
        }

        lastColorTag?.Value(run, lastColorTag.Value.Key);
        return run;
    }

    protected override Type StyleKeyOverride { get; } = typeof(TextBlock);
}
