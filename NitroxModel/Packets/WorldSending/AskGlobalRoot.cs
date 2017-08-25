using System;

namespace NitroxModel.Packets.WorldSending
{
    [Serializable]
    public class AskGlobalRoot : AuthenticatedPacket
    {
        public AskGlobalRoot(string playerId) : base(playerId) {}
    }
}
