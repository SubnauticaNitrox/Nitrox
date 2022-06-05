using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[JsonContractTransition]
public class EntitySignMetadata : EntityMetadata
{
    [JsonMemberTransition]
    public string Text { get; set; }

    [JsonMemberTransition]
    public int ColorIndex { get; set; }

    [JsonMemberTransition]
    public int ScaleIndex { get; set; }

    [JsonMemberTransition]
    public bool[] Elements { get; set; }

    [JsonMemberTransition]
    public bool Background { get; set; }

    public EntitySignMetadata(string text, int colorIndex, int scaleIndex, bool[] elements, bool background)
    {
        Text = text;
        ColorIndex = colorIndex;
        ScaleIndex = scaleIndex;
        Elements = elements;
        Background = background;
    }

    public override string ToString()
    {
        return $"[EntitySignMetadata - Text: {Text}, ColorIndex: {ColorIndex}, ScaleIndex: {ScaleIndex}, Elements: {string.Join("; ", Elements)}, Background: {Background}]";
    }
}
