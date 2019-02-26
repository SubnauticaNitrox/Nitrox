using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsDecoyLaunch : Packet
    {
        public string Guid { get; }

        public CyclopsDecoyLaunch(string guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsDecoyLaunch Guid: " + Guid + "]";
        }
    }
}
