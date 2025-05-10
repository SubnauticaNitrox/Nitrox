using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record ToggleLights : Packet
    {
        public NitroxId Id { get; }
        public bool IsOn { get; }

        public ToggleLights(NitroxId id, bool isOn)
        {
            Id = id;
            IsOn = isOn;
        }
    }
}
