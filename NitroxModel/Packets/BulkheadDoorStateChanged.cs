using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;


[Serializable]
public sealed class BulkheadDoorStateChanged : Packet
{
    public NitroxId Id { get; }
    public bool IsOpen { get; }

    public BulkheadDoorStateChanged(NitroxId id, bool isOpen)
    {
        Id = id;
        IsOpen = isOpen;
    }
}
