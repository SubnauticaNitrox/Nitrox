using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable, DataContract]
public class PlayerAnimation
{
    [DataMember(Order = 1)]
    public int Type { get; set; }

    [DataMember(Order = 2)]
    public int State { get; }

    [IgnoreConstructor]
    protected PlayerAnimation()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PlayerAnimation(int type, int state)
    {
        Type = type;
        State = state;
    }

    public override string ToString()
    {
        return $"[{nameof(PlayerAnimation)} Type: {Type}, State: {State}]";
    }
}
