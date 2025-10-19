using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;


[Serializable]
public sealed class BulkheadDoorStateChanged : Packet
{
    public NitroxId Id { get; }

    public ushort PlayerId { get; }

    public bool IsOpen { get; }

    public bool IsFacingDoor { get; }

    public BulkheadDoorStateChanged(NitroxId id, ushort playerId, bool isOpen, bool isFacingDoor)
    {
        Id = id;
        PlayerId = playerId;
        IsOpen = isOpen;
        IsFacingDoor = isFacingDoor;
    }
}
