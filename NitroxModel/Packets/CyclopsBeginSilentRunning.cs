using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsBeginSilentRunning : AuthenticatedPacket
    {
        public string Guid { get; }

        public CyclopsBeginSilentRunning(string playerId, string guid) : base(playerId)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[CyclopsBeginSilentRunning PlayerId: " + PlayerId + " Guid: " + Guid + "]";
        }
    }
}
