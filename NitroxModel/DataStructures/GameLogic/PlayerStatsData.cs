using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [DataContract]
    public class PlayerStatsData
    {
        [DataMember(Order = 1)]
        public float Oxygen { get; }

        [DataMember(Order = 2)]
        public float MaxOxygen { get; }

        [DataMember(Order = 3)]
        public float Health { get; }

        [DataMember(Order = 4)]
        public float Food { get; }

        [DataMember(Order = 5)]
        public float Water { get; }
#if SUBNAUTICA
        [DataMember(Order = 6)]
        public float InfectionAmount { get; }
#endif

        [IgnoreConstructor]
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
            return $"[Oxygen: {Oxygen} MaxOxygen: {MaxOxygen} Health: {Health} Food: {Food} Water: {Water} InfectionAmount: {InfectionAmount} ]";
#elif BELOWZERO
            return $"[Oxygen: {Oxygen} MaxOxygen: {MaxOxygen} Health: {Health} Food: {Food} Water: {Water} ]";
#endif
        }
    }
}
