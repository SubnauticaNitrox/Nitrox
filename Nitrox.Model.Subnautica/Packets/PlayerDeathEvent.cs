using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
{
    [Serializable]
    public class PlayerDeathEvent : Packet
    {
        public SessionId SessionId { get; }
        public NitroxVector3 DeathPosition { get; }

        public PlayerDeathEvent(SessionId sessionId, NitroxVector3 deathPosition)
        {
            SessionId = sessionId;
            DeathPosition = deathPosition;
        }
    }
}
