using System;
using NitroxModel.Networking;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerStats : Packet
    {
        public ushort PlayerId { get; }
        public float Oxygen { get; }
        public float MaxOxygen { get; }
        public float Health { get; }
        public float Food { get; }
        public float Water { get; }

        public PlayerStats(ushort playerId, float oxygen, float maxOxygen, float health, float food, float water)
        {
            PlayerId = playerId;
            Oxygen = oxygen;
            MaxOxygen = maxOxygen;
            Health = health;
            Food = food;
            Water = water;
            DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UnreliableSequenced;
            UdpChannel = UdpChannelId.PLAYER_STATS;
        }

        public override string ToString()
        {
            return "[PlayerStats - PlayerId: " + PlayerId + " Oxygen: " + Oxygen + " MaxOxygen:" + MaxOxygen + " Health: " + Health + " Food: " + Food + " Water: " + Water + "]";
        }
    }
}
