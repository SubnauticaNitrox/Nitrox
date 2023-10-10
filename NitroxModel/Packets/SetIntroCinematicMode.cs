using System;

namespace NitroxModel.Packets;

[Serializable]
public class SetIntroCinematicMode : Packet
{
    public enum IntroCinematicMode
    {
        LOADING,
        WAITING,
        START,
        COMPLETED
    }
    public ushort PlayerId { get; }
    public IntroCinematicMode Mode { get; }

    public SetIntroCinematicMode(ushort playerId, IntroCinematicMode mode)
    {
        PlayerId = playerId;
        Mode = mode;
    }
}
