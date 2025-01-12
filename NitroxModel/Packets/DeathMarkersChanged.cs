namespace NitroxModel.Packets;

public class DeathMarkersChanged : Packet
{
    public bool MarkDeathPointsWithBeacon { get; }

    public DeathMarkersChanged(bool markDeathPointsWithBeacon)
    {
        MarkDeathPointsWithBeacon = markDeathPointsWithBeacon;
    }
}
