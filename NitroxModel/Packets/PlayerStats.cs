using NitroxModel.Networking;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayerStats : Packet
    {
        [Index(0)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(1)]
        public virtual float Oxygen { get; protected set; }
        [Index(2)]
        public virtual float MaxOxygen { get; protected set; }
        [Index(3)]
        public virtual float Health { get; protected set; }
        [Index(4)]
        public virtual float Food { get; protected set; }
        [Index(5)]
        public virtual float Water { get; protected set; }
        [Index(6)]
        public virtual float InfectionAmount { get; protected set; }

        public PlayerStats() { }

        public PlayerStats(ushort playerId, float oxygen, float maxOxygen, float health, float food, float water, float infectionAmount)
        {
            PlayerId = playerId;
            Oxygen = oxygen;
            MaxOxygen = maxOxygen;
            Health = health;
            Food = food;
            Water = water;
            InfectionAmount = infectionAmount;
            DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
            UdpChannel = UdpChannelId.PLAYER_STATS;
        }
    }
}
