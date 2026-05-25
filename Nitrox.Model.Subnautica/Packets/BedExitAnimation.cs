using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

/// <summary>
/// Sent when a player starts the bed stand-up animation.
/// </summary>
[Serializable]
public class BedExitAnimation : Packet
{
    public SessionId SessionId { get; }
    public NitroxId BedId { get; }
    public string AnimationKey { get; } // "bed_up_left" or "bed_up_right"

    public BedExitAnimation(SessionId sessionId, NitroxId bedId, string animationKey)
    {
        SessionId = sessionId;
        BedId = bedId;
        AnimationKey = animationKey;
    }
}
