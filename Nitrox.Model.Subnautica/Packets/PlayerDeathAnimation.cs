using System;
using Nitrox.Model.Core;
using Nitrox.Model.GameLogic.PlayerAnimation;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerDeathAnimation(SessionId sessionId, PlayerDeathAnimationType animationType) : Packet
{
    public SessionId SessionId { get; } = sessionId;
    public PlayerDeathAnimationType AnimationType { get; } = animationType;
}
