using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsActivateHorn : Packet
    {
        public string Guid { get; }

        public CyclopsActivateHorn(string guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsActivateHorn Guid: " + Guid + "]";
        }
    }
}
