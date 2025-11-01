using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.GameLogic.PlayerAnimation;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable, DataContract]
public class PlayerAnimation
{
    [DataMember(Order = 1)]
    public AnimChangeType Type { get; set; }

    [DataMember(Order = 2)]
    public AnimChangeState State { get; }

    [IgnoreConstructor]
    protected PlayerAnimation()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public PlayerAnimation(AnimChangeType type, AnimChangeState state)
    {
        Type = type;
        State = state;
    }

    public override string ToString()
    {
        return $"[{nameof(PlayerAnimation)} Type: {Type}, State: {State}]";
    }
}
