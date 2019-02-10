using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsToggleEngineState : Packet
    {
        public string Guid { get; }
        public bool IsOn { get; }
        public bool IsStarting { get; }

        public CyclopsToggleEngineState(string guid, bool isOn, bool isStarting)
        {
            Guid = guid;
            IsOn = isOn;
            IsStarting = isStarting;
        }

        public override string ToString()
        {
            return "[CyclopsToggleEngineState Guid: " + Guid + " IsOn: " + IsOn + " IsStarting: " + IsStarting + "]";
        }
    }
}
