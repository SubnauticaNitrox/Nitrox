using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsSonarPing : Packet
    {
        public string Guid { get; }

        public CyclopsSonarPing(string guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsSonarPing Guid: " + Guid + "]";
        }
    }
}
