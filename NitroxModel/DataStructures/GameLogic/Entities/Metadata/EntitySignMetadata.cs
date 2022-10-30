using System;
using BinaryPack.Attributes;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[ProtoContract]
public class EntitySignMetadata : EntityMetadata
{
    [ProtoMember(1)]
    public string Text { get; }

    [ProtoMember(2)]
    public int ColorIndex { get; }

    [ProtoMember(3)]
    public int ScaleIndex { get; }

    [ProtoMember(4)]
    public bool[] Elements { get; }

    [ProtoMember(5)]
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
