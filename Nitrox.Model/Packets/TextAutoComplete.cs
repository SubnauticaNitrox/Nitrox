using System;

namespace Nitrox.Model.Packets;

[Serializable]
public sealed class TextAutoComplete(string? text, TextAutoComplete.AutoCompleteContext context) : Packet
{
    /// <summary>
    ///     Text to send over as either a suggestion for auto complete (from client) or a reply to the suggestion (from
    ///     server).
    /// </summary>
    public string? Text { get; init; } = text;

    public AutoCompleteContext Context { get; init; } = context;

    public enum AutoCompleteContext
    {
        COMMAND_NAME
    }
}
