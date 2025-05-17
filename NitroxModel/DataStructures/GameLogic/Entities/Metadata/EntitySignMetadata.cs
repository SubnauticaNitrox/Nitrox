using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class EntitySignMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public string Text { get; }

    [DataMember(Order = 2)]
    public int ColorIndex { get; }

    [DataMember(Order = 3)]
    public int ScaleIndex { get; }

    [DataMember(Order = 4)]
    public bool[] Elements { get; } = [];

    [DataMember(Order = 5)]
    public bool Background { get; }

    [IgnoreConstructor]
    protected EntitySignMetadata()
    {
        //Constructor for serialization. Has to be "protected" for json serialization.
    }

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
        return $"[EntitySignMetadata - Text: {Text}, ColorIndex: {ColorIndex}, ScaleIndex: {ScaleIndex}, Elements: {Elements}, Background: {Background}]";
    }
}
