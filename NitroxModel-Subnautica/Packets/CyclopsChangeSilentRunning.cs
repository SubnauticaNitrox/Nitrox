using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsChangeSilentRunning : Packet
    {
        public string Guid { get; }
        public bool IsOn { get; }

        public CyclopsChangeSilentRunning(string guid, bool isOn)
        {
            Guid = guid;
            IsOn = isOn;
        }

        public override string ToString()
        {
            return "[CyclopsBeginSilentRunning Guid: " + Guid + " , IsOn: " + IsOn + "]";
        }
    }
}
