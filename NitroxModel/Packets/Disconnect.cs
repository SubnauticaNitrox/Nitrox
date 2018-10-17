using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Disconnect : Packet
    {
        public ulong LPlayerId { get; }

        public Disconnect(ulong playerId)
        {
            LPlayerId = playerId;
        }
    }
}
