using System;

namespace NitroxModel.Packets.WorldSending
{
    [Serializable]
    public class ReceiveGlobalRoot : AuthenticatedPacket
    {
        public byte[] GlobalRootData { get; private set; }

        public ReceiveGlobalRoot(string playerId, byte[] globalRootData) : base(playerId)
        {
            this.GlobalRootData = globalRootData;
        }
    }
}
