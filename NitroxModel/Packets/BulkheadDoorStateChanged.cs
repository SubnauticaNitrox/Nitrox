using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;


[Serializable]
public sealed class BulkheadDoorStateChanged : Packet
{
    public NitroxId Id { get; }
    public bool IsOpen { get; }
    public ushort PlayerId { get; }

    public BulkheadDoorStateChanged(NitroxId id, bool isOpen, ushort playerId)
    {
        Id = id;
        IsOpen = isOpen;
        PlayerId = playerId;
    }
}
