using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerStats : AuthenticatedPacket
    {
        public float Oxygen { get; }
        public float MaxOxygen { get; }
        public float Health { get; }
        public float Food { get; }
        public float Water { get; }

        public PlayerStats(String playerId, float oxygen, float maxOxygen, float health, float food, float water) : base(playerId)
        {
            Oxygen = oxygen;
            MaxOxygen = maxOxygen;
            Health = health;
            Food = food;
            Water = water;
        }

        public override string ToString()
        {
            return "[PlayerStats - Oxygen: " + Oxygen + " MaxOxygen:" + MaxOxygen + " Health: " + Health + " Food: " + Food + " Water: " + Water + "]";
        }
    }
}
