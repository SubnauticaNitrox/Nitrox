using NitroxModel.DataStructures.Unity;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayerDeathEvent : Packet
    {
        [Index(0)]
        public virtual string PlayerName { get; protected set; }
        [Index(1)]
        public virtual NitroxVector3 DeathPosition { get; protected set; }

        private PlayerDeathEvent() { }

        public PlayerDeathEvent(string playerName, NitroxVector3 deathPosition)
        {
            PlayerName = playerName;
            DeathPosition = deathPosition;
        }
    }
}
