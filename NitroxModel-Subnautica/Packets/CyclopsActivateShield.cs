using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsActivateShield : Packet
    {
        public string Guid { get; }

        public CyclopsActivateShield(string guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsActivateShield Guid: " + Guid + "]";
        }
    }
}
