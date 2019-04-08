using System;
using NitroxModel.Packets;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsSonarPing : Packet
    {
        public NitroxId Id { get; }

        public CyclopsSonarPing(NitroxId id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return "[CyclopsSonarPing Id: " + Id + "]";
        }
    }
}
