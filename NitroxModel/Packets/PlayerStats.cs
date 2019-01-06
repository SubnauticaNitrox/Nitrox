using Lidgren.Network;
using System;
using NitroxModel.MultiplayerSession;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerStats : Packet
    {
        public PlayerContext PlayerContext;
        public float Oxygen { get; }
        public float MaxOxygen { get; }
        public float Health { get; }
        public float Food { get; }
        public float Water { get; }

        public PlayerStats(PlayerContext playerContext, float oxygen, float maxOxygen, float health, float food, float water)
        {
            PlayerContext = playerContext;
            Oxygen = oxygen;
            MaxOxygen = maxOxygen;
            Health = health;
            Food = food;
            Water = water;
            DeliveryMethod = NetDeliveryMethod.UnreliableSequenced;
            UdpChannel = UdpChannelId.PLAYER_STATS;
        }

        public override string ToString()
        {
            return "[PlayerStats - PlayerId: " + PlayerContext.PlayerId + " Oxygen: " + Oxygen + " MaxOxygen:" + MaxOxygen + " Health: " + Health + " Food: " + Food + " Water: " + Water + "]";
        }
    }
}
