using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class RocketEndLaunch : Packet
    {
        public NitroxId Id { get; }

        public RocketEndLaunch(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"[RocketEndLaunch - Id: {Id}]";
        }
    }
}
