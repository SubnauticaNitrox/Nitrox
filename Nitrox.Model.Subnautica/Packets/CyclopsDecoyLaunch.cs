using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
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
