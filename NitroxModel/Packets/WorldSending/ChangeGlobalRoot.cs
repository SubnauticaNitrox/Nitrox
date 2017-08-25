using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ChangeGlobalRoot : AuthenticatedPacket
    {
        public byte[] GlobalRootData { get; private set; }

        public ChangeGlobalRoot(string playerId, byte[] globalRootData) : base(playerId)
        {
            this.GlobalRootData = globalRootData;
        }
    }
}