using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Nitrox.Launcher.UI.Controls;

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
    private static readonly Dictionary<string, Action<Run>> tagActions = new()
    {
        { "u", run => run.TextDecorations = run.TextDecorations = [new() { Location = TextDecorationLocation.Underline }] },
        { "b", run => run.FontWeight = FontWeight.Bold },
        { "i", run => run.FontStyle = FontStyle.Italic }
    };

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
    private static partial Regex TagParserRegex();

    private void ParseTextAndAddInlines(string text)
    {
        static string GetSubText(string text, Match lastMatch, Match currentMatch = null)
        {
            int start = lastMatch == null ? 0 : lastMatch.Index + lastMatch.Length;
            int length = (currentMatch?.Index ?? text.Length) - start;
            return length > 0 ? text.Substring(start, length) : "";
        }

        if (Inlines == null)
        {
            return;
        }

        MatchCollection matches = TagParserRegex().Matches(text);
        if (matches.Count == 0)
        {
            Inlines?.Add(new Run(text));
        }
        else
        {
            Match lastMatch = null;
            HashSet<string> activeTags = [];
            foreach (Match match in matches)
            {
                string subText = GetSubText(text, lastMatch, match);
                if (!string.IsNullOrEmpty(subText))
                {
                    Inlines.Add(CreateRunWithTags(subText, activeTags));
                }

                switch (match)
                {
                    case { ValueSpan: ['[', '/', ..], Groups: [_, { Value: var tag }, ..] }:
                        activeTags.Remove(tag);
                        break;
                    case { Groups: [_, { Value: var name }, { Value: { Length: > 0 } url }] }:
                        TextBlock textBlock = new();
                        textBlock.Classes.Add("link");
                        textBlock.Text = name;
                        textBlock.Tag = url;
                        Inlines.Add(textBlock);
                        break;
                    case { Groups: [_, { Value: var tag }, ..] }:
                        activeTags.Add(tag);
                        break;
                }

                lastMatch = match;
            }

            // Handle text that comes after last end tag.
            string lastPart = GetSubText(text, lastMatch);
            if (!string.IsNullOrEmpty(lastPart))
            {
                Inlines?.Add(CreateRunWithTags(lastPart, activeTags));
            }
        }
    }

    private Run CreateRunWithTags(string text, HashSet<string> tags)
    {
        ArgumentNullException.ThrowIfNull(text);

        Run run = new(text);
        foreach (string tag in tags)
        {
            if (tagActions.TryGetValue(tag, out Action<Run> action))
            {
                action(run);
            }
        }
        return run;
    }

    protected override Type StyleKeyOverride { get; } = typeof(TextBlock);
}
