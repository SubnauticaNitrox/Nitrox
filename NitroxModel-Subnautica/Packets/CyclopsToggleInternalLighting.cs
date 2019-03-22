using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsToggleInternalLighting : Packet
    {
        public string Guid { get; }
        public bool IsOn { get; }

        public CyclopsToggleInternalLighting(string guid, bool isOn)
        {
            Guid = guid;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return "[CyclopsToggleInternalLighting Guid: " + Guid + " IsOn: " + IsOn + "]";
        }
    }
}
