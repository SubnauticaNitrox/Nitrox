using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class DiveReelNodesReset : Packet
{
    public ushort PlayerId { get; }

    public DiveReelNodesReset(ushort playerId)
    {
        PlayerId = playerId;
    }

    public override string ToString()
    {
        return $"[{nameof(DiveReelNodesReset)} PlayerId: {PlayerId}]";
    }
}
