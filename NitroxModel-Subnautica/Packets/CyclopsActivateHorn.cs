using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsActivateHorn : Packet
    {
        public NitroxId Id { get; }

        public CyclopsActivateHorn(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"[CyclopsActivateHorn - Id: {Id}]";
        }
    }
}
