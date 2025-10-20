using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
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
    }
}
