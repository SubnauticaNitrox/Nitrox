using System;
using NitroxModel.Networking;

namespace NitroxModel.Packets;

[Serializable]
public class PlayerStats : Packet
{
    public ushort PlayerId { get; set; }
    public float Oxygen { get; }
    public float MaxOxygen { get; }
    public float Health { get; }
    public float Food { get; }
    public float Water { get; }
#if SUBNAUTICA
    public float InfectionAmount { get; }

    public PlayerStats(ushort playerId, float oxygen, float maxOxygen, float health, float food, float water, float infectionAmount)
#elif BELOWZERO
    public PlayerStats(ushort playerId, float oxygen, float maxOxygen, float health, float food, float water)
#endif
    {
        PlayerId = playerId;
        Oxygen = oxygen;
        MaxOxygen = maxOxygen;
        Health = health;
        Food = food;
        Water = water;
#if SUBNAUTICA
        InfectionAmount = infectionAmount;
#endif
        DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.RELIABLE_ORDERED_LAST;
    }
}
