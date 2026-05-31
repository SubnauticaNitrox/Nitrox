using System;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class AnimationChangeEvent(SessionId sessionId, PlayerAnimation animation) : Packet
{
    public SessionId SessionId { get; } = sessionId;
    public PlayerAnimation Animation { get; } = animation;
}
