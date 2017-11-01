using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsToggleFloodLights : Packet
    {
        public string Guid { get; }
        public bool IsOn { get; }

        public CyclopsToggleFloodLights(string guid, bool isOn)
        {
            Guid = guid;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return "[CyclopsToggleFloodLights Guid: " + Guid + " IsOn: " + IsOn + "]";
        }
    }
}
