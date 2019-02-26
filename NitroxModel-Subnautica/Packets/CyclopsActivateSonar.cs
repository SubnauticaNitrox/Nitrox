using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsActivateSonar : Packet
    {
        public string Guid { get; }

        public CyclopsActivateSonar(string guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsActivateSonar Guid: " + Guid + "]";
        }
    }
}
