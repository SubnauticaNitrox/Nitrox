using System;
using NitroxModel.Packets;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsDestroyed : Packet
    {
        public NitroxId Id { get; }

        public CyclopsDestroyed(NitroxId id)
        {
            Id = id;
        }
    }
}
