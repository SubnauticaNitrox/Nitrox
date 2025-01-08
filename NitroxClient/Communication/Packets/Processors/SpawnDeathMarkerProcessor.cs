using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;
public class SpawnDeathMarkerProcessor : ClientPacketProcessor<SpawnDeathMarker>
{
    public override void Process(SpawnDeathMarker packet)
    {
        if (!DefaultWorldEntitySpawner.TryCreateGameObjectSync(TechType.Beacon, "7b019de0-db51-4017-8812-2531b808228d", new NitroxId(), out GameObject createdBeacon))
        {
            Log.Error("!!! Error occured in spawning death beacon");
        }
        createdBeacon.transform.position = packet.spawnPosition.ToUnity();
    }
}
