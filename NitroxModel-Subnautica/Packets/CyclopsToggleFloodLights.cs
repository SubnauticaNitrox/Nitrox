using System;
using NitroxModel.Packets;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsToggleFloodLights : Packet
    {
        public NitroxId Id { get; }
        public bool IsOn { get; }

        public CyclopsToggleFloodLights(NitroxId id, bool isOn)
        {
            Id = id;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return "[CyclopsToggleFloodLights Id: " + Id + " IsOn: " + IsOn + "]";
        }
    }
}
