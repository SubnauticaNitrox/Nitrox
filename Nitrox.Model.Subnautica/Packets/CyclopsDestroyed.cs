using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
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
