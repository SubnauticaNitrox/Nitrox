using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class PlayerStatsData
    {
        [ProtoMember(1)]
        public float Oxygen { get; }

        [ProtoMember(2)]
        public float MaxOxygen { get; }

        [ProtoMember(3)]
        public float Health { get; }

        [ProtoMember(4)]
        public float Food { get; }

        [ProtoMember(5)]
        public float Water { get; }
#if SUBNAUTICA
        [ProtoMember(6)]
        public float InfectionAmount { get; }
#endif
        protected PlayerStatsData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }
#if SUBNAUTICA
        public PlayerStatsData(float oxygen, float maxOxygen, float health, float food, float water, float infectionAmount)
#elif BELOWZERO
        public PlayerStatsData(float oxygen, float maxOxygen, float health, float food, float water)
#endif
        {
            Oxygen = oxygen;
            MaxOxygen = maxOxygen;
            Health = health;
            Food = food;
            Water = water;
#if SUBNAUTICA
            InfectionAmount = infectionAmount;
#endif
        }

        public override string ToString()
        {
#if SUBNAUTICA
            return "[Oxygen: " + Oxygen + " MaxOxygen: " + MaxOxygen + " Health: " + Health + " Food: " + Food + " Water: " + Water + " InfectionAmount: " + InfectionAmount + " ]";
#elif BELOWZERO
            return "[Oxygen: " + Oxygen + " MaxOxygen: " + MaxOxygen + " Health: " + Health + " Food: " + Food + " Water: " + Water + " ]";
#endif
        }
    }
}
