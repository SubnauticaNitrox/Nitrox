using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class BatchVisibilityChanged : Packet
{
    public ushort PlayerId { get; }
    public NitroxInt3[] Added { get; }
    public NitroxInt3[] Removed { get; }

    public BatchVisibilityChanged(ushort playerId, NitroxInt3[] added, NitroxInt3[] removed)
    {
        PlayerId = playerId;
        Added = added;
        Removed = removed;
    }
}
