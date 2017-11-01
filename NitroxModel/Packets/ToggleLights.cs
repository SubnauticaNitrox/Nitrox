using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ToggleLights : Packet
    {
        public string Guid { get; }
        public bool IsOn { get; }

        public ToggleLights(string guid, bool isOn)
        {
            Guid = guid;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return "[ToggleLightsPacket Guid: " + Guid + " IsOn: " + IsOn + "]";
        }
    }
}
