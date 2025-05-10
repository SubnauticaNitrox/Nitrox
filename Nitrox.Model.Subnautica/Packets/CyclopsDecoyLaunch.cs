using System;
using NitroxModel.DataStructures;
using NitroxModel.Networking.Packets;

namespace Nitrox.Model.Subnautica.Packets
{
    [Serializable]
    public record CyclopsDecoyLaunch : Packet
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
