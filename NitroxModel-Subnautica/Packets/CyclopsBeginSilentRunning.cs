using System;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class CyclopsBeginSilentRunning : Packet
    {
        public string Guid { get; }

        public CyclopsBeginSilentRunning(string guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsBeginSilentRunning Guid: " + Guid + "]";
        }
    }
}
