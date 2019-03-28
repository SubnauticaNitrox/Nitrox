using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsChangeSonarMode : Packet
    {
        public string Guid { get; }
        public bool Active { get; }

        public CyclopsChangeSonarMode(string guid)
        {
            Guid = guid;
            Active = false;
        }
        public CyclopsChangeSonarMode(string guid, bool active)
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
