using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class ToggleLights : Packet
    {
        public NitroxId Id { get; }
        public bool IsOn { get; }

        public ToggleLights(NitroxId id, bool isOn)
        {
            Id = id;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return "[ToggleLightsPacket Id: " + Id + " IsOn: " + IsOn + "]";
        }
    }
}
