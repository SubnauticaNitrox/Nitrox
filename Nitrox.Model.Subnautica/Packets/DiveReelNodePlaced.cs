using System;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class DiveReelNodePlaced : Packet
{
    public ushort PlayerId { get; }
    public NitroxVector3 Position { get; }
    public bool IsFirst { get; }

    public DiveReelNodePlaced(ushort playerId, NitroxVector3 position, bool isFirst)
    {
        PlayerId = playerId;
        Position = position;
        IsFirst = isFirst;
    }

    public override string ToString()
    {
        return $"[{nameof(DiveReelNodePlaced)} PlayerId: {PlayerId}, Position: {Position}, IsFirst: {IsFirst}]";
    }
}
