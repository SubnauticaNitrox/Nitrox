using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsChangeShieldMode : Packet
    {
        public string Guid { get; }
        public bool IsOn { get; }

        public CyclopsChangeShieldMode(string guid, bool isOn)
        {
            Guid = guid;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return "[CyclopsActivateShield Guid: " + Guid + " activate: " + IsOn + "]";
        }
    }
}
