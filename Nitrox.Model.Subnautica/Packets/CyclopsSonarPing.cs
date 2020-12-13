using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
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
            return $"[CyclopsSonarPing - Id: {Id}]";
        }
    }
}
