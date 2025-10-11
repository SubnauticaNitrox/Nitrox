using System;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets;

[Serializable]
public class AnimationChangeEvent : Packet
{
    public ushort PlayerId { get; }
    public PlayerAnimation Animation { get; }

    public AnimationChangeEvent(ushort playerId, PlayerAnimation animation)
    {
        PlayerId = playerId;
        Animation = animation;
    }
}
