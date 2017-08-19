using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Disconnect : AuthenticatedPacket
    {
        public Disconnect(String playerId) : base(playerId) {}
    }
}
