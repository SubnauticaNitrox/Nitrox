using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class CyclopsRestored : Packet
{
    public NitroxId Id { get; }

    public CyclopsRestored(NitroxId id)
    {
        Id = id;
    }
}
