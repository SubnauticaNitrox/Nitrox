using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets;
public class SpawnDeathMarker : Packet
{
    public NitroxVector3 spawnPosition { get; }

    public SpawnDeathMarker(NitroxVector3 spawnPosition)
    {
        this.spawnPosition = spawnPosition;
    }
}
