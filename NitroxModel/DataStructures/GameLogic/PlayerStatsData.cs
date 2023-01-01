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
        [DataMember(Order = 6)]
        public float InfectionAmount { get; }

        [IgnoreConstructor]
        protected PlayerStatsData()
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
            return $"[Oxygen: {Oxygen} MaxOxygen: {MaxOxygen} Health: {Health} Food: {Food} Water: {Water} InfectionAmount: {InfectionAmount} ]";
        }
    }
}
