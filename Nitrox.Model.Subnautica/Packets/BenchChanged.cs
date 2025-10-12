using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class BenchChanged : Packet
{
    public ushort PlayerId { get; }
    public NitroxId BenchId { get; }
    public BenchChangeState ChangeState { get; }

    public BenchChanged(ushort playerId, NitroxId benchId, BenchChangeState changeState)
    {
        PlayerId = playerId;
        BenchId = benchId;
        ChangeState = changeState;
    }

    public enum BenchChangeState
    {
        SITTING_DOWN,
        STANDING_UP,
        UNSET
    }
}
