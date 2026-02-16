using System;
using Nitrox.Model.Core;
using Nitrox.Model.Networking;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerStats : Packet
{
    public SessionId SessionId { get; set; }
    public float Oxygen { get; }
    public float MaxOxygen { get; }
    public float Health { get; }
    public float Food { get; }
    public float Water { get; }
    public float InfectionAmount { get; }

    public PlayerStats(SessionId sessionId, float oxygen, float maxOxygen, float health, float food, float water, float infectionAmount)
    {
        SessionId = sessionId;
        Oxygen = oxygen;
        MaxOxygen = maxOxygen;
        Health = health;
        Food = food;
        Water = water;
        InfectionAmount = infectionAmount;

        DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.RELIABLE_ORDERED_LAST;
    }
}
