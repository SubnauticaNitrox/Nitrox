using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class RecieveGlobalRoot : AuthenticatedPacket
    {
        public byte[] GlobalRootData { get; private set; }

        public RecieveGlobalRoot(string playerId, byte[] globalRootData) : base(playerId)
        {
            this.GlobalRootData = globalRootData;
        }
    }
}
