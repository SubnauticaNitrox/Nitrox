using System;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets;

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
