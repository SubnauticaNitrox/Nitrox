using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

/// <summary>
/// Sent when a player starts the bed lie-down animation.
/// </summary>
[Serializable]
public class BedEnterAnimation : Packet
{
    public SessionId SessionId { get; }
    public NitroxId BedId { get; }
    public string AnimationKey { get; } // "bed_down_left" or "bed_down_right"

    public BedEnterAnimation(SessionId sessionId, NitroxId bedId, string animationKey)
    {
        SessionId = sessionId;
        BedId = bedId;
        AnimationKey = animationKey;
    }
}
