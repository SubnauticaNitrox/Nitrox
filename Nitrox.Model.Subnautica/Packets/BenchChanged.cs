using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class BenchChanged : Packet
{
    public SessionId SessionId { get; }
    public NitroxId BenchId { get; }
    public BenchChangeState ChangeState { get; }

    public BenchChanged(SessionId sessionId, NitroxId benchId, BenchChangeState changeState)
    {
        SessionId = sessionId;
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
