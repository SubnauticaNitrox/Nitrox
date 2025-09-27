using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;


[Serializable]
public sealed class BulkheadDoorStateChanged : Packet
{
    public NitroxId Id { get; }

    public ushort PlayerId { get; }

    public bool IsOpen { get; }

    public BulkheadDoorStateChanged(NitroxId id, ushort playerId, bool isOpen)
    {
        Id = id;
        PlayerId = playerId;
        IsOpen = isOpen;
    }
}
