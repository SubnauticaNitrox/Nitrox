﻿using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
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
