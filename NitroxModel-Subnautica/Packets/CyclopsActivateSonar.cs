using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsActivateSonar : Packet
    {
        public string Guid { get; }
        public bool Active { get; }

        public CyclopsActivateSonar(string guid)
        {
            Guid = guid;
            Active = false;
        }
        public CyclopsActivateSonar(string guid, bool active)
        {
            Guid = guid;
            Active = active;
        }

        public override string ToString()
        {
            return "[CyclopsActivateSonar Guid: " + Guid + "," + "active: " + Active + "]";
        }
    }
}
