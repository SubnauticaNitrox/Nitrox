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
        CoroutineHost.StartCoroutine(SpawnQueue());
    }

    public override void Process(SpawnEntities packet)
    {
        if (packet.ForceRespawn)
        {
            CleanupExistingEntities(packet.Entities);
        }

        entities.EntitiesToSpawn.AddRange(packet.Entities);
    }

    public IEnumerator SpawnQueue()
    {
        while (true)
        {
            if (!spawning && entities.EntitiesToSpawn.Count > 0)
            {
                spawning = true;
                yield return entities.SpawnBatchAsync(entities.EntitiesToSpawn).OnYieldError(Log.Error);
                spawning = false;
            }
            yield return null;
        }
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
