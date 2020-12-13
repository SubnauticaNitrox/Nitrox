using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
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
