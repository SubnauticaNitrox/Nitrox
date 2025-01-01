using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class KeypadMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool Unlocked { get; }

    [DataMember(Order = 2)]
    public string PathFromRoot { get; }

    [IgnoreConstructor]
    protected KeypadMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public KeypadMetadata(bool unlocked, string pathFromRoot)
    {
        Unlocked = unlocked;
        PathFromRoot = pathFromRoot;
    }

    public override string ToString()
    {
        return $"[{nameof(KeypadMetadata)} Unlocked: {Unlocked}, PathFromRoot: {PathFromRoot}]";
    }
}
