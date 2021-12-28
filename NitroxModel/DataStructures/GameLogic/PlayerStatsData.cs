using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    [ProtoContract]
    public class PlayerStatsData
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual float Oxygen { get; protected set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual float MaxOxygen { get; protected set; }

        [Index(2)]
        [ProtoMember(3)]
        public virtual float Health { get; protected set; }

        [Index(3)]
        [ProtoMember(4)]
        public virtual float Food { get; protected set; }

        [Index(4)]
        [ProtoMember(5)]
        public virtual float Water { get; protected set; }

        [Index(5)]
        [ProtoMember(6)]
        public virtual float InfectionAmount { get; protected set; }

        public PlayerStatsData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public PlayerStatsData(float oxygen, float maxOxygen, float health, float food, float water, float infectionAmount)
        {
            Oxygen = oxygen;
            MaxOxygen = maxOxygen;
            Health = health;
            Food = food;
            Water = water;
            InfectionAmount = infectionAmount;
        }

        public override string ToString()
        {
            return "[Oxygen: " + Oxygen + " MaxOxygen: " + MaxOxygen + " Health: " + Health + " Food: " + Food + " Water: " + Water + " InfectionAmount: " + InfectionAmount + " ]";
        }
    }
}
