using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class DeathMarkersChanged : Packet
{
    public bool MarkDeathPointsWithBeacon { get; }

    public DeathMarkersChanged(bool markDeathPointsWithBeacon)
    {
        MarkDeathPointsWithBeacon = markDeathPointsWithBeacon;
    }
}
