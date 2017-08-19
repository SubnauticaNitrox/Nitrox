using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsBeginSilentRunning : AuthenticatedPacket
    {
        public String Guid { get; private set; }

        public CyclopsBeginSilentRunning(String playerId, String guid) : base(playerId)
        {
            this.Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsBeginSilentRunning PlayerId: " + PlayerId + " Guid: " + Guid + "]";
        }
    }
}
