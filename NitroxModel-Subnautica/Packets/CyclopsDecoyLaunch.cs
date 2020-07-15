using System;
using NitroxModel.Packets;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsDecoyLaunch : Packet
    {
        public NitroxId Id { get; }

        public CyclopsDecoyLaunch(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"[CyclopsDecoyLaunch - Id: {Id}]";
        }
    }
}
