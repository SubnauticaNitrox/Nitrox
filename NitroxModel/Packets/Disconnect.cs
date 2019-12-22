using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Disconnect : Packet
    {
        public NitroxId PlayerId { get; }

        public Disconnect(NitroxId playerId)
        {
            PlayerId = playerId;
        }
    }
}
