
using System;
using ProtoBuf;

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

        [ProtoMember(4)]
        public float Water { get; }

        public PlayerStatsData(float oxygen, float maxOxygen, float health, float food, float water)
        {
            Oxygen = oxygen;
            MaxOxygen = maxOxygen;
            Health = health;
            Food = food;
            Water = water;
        }
    }
}
