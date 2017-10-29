using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Disconnect : AuthenticatedPacket
    {
        public Disconnect(string playerId) : base(playerId) {}
    }
}
