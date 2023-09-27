using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;
using UWE;

namespace NitroxClient.Communication.Packets.Processors;

public class SpawnEntitiesProcessor : ClientPacketProcessor<SpawnEntities>
{
    private readonly Entities entities;
    private bool spawning;

    public SpawnEntitiesProcessor(Entities entities)
    {
        this.entities = entities;
    }

    public override void Process(SpawnEntities packet)
    {
        if (packet.ForceRespawn)
        {
            CleanupExistingEntities(packet.Entities);
        }

        if (packet.Entities.Count > 0)
        {
            entities.EntitiesToSpawn.AddRange(packet.Entities);
            // Packet processing is done in the main thread so there's no issue with using the spawning bool like so
            if (!spawning)
            {
                spawning = true;
                CoroutineHost.StartCoroutine(SpawnNewEntities());
            }
        }
    }

    public IEnumerator SpawnNewEntities()
    {
        yield return entities.SpawnBatchAsync(entities.EntitiesToSpawn).OnYieldError(Log.Error);
        spawning = false;
    }

    private void CleanupExistingEntities(List<Entity> dirtyEntities)
    {
        foreach (Entity entity in dirtyEntities)
        {
            entities.RemoveEntityHierarchy(entity);

            Optional<GameObject> gameObject = NitroxEntity.GetObjectFrom(entity.Id);

            if (gameObject.HasValue)
            {
                UnityEngine.Object.Destroy(gameObject.Value);
            }
        }
    }
}
