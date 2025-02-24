namespace NitroxModel.Packets;

[Serializable]
public class DeathMarkersChanged : Packet
{
    public bool MarkDeathPointsWithBeacon { get; }

    public DeathMarkersChanged(bool markDeathPointsWithBeacon)
    {
        MarkDeathPointsWithBeacon = markDeathPointsWithBeacon;
    }
}
