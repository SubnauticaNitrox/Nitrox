using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

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

        public override string ToString()
        {
            return $"[CyclopsDestroyed - Id: {Id}]";
        }
    }
}
