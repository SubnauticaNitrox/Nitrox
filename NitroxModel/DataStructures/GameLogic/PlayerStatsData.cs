using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class PlayerStatsData
{
    [JsonMemberTransition]
    public float Oxygen { get; }
    [JsonMemberTransition]
    public float MaxOxygen { get; }
    [JsonMemberTransition]
    public float Health { get; }
    [JsonMemberTransition]
    public float Food { get; }
    [JsonMemberTransition]
    public float Water { get; }
    [JsonMemberTransition]
    public float InfectionAmount { get; }

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
        return $"[PlayerStatsData - Oxygen: {Oxygen}, MaxOxygen: {MaxOxygen}, Health: {Health}, Food: {Food}, Water: {Water}, InfectionAmount: {InfectionAmount}]";
    }
}
