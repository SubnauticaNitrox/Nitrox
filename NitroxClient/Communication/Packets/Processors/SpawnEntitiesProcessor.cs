using System;
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
    private readonly List<Entity> entitiesToSpawn;
    private bool spawning;
    private Dictionary<Type, int> spawnCount;

    public SpawnEntitiesProcessor(Entities entities)
    {
        this.entities = entities;
        entitiesToSpawn = new();
        spawnCount = new();
        CoroutineHost.StartCoroutine(SpawnQueue());
    }

    public override void Process(SpawnEntities packet)
    {
        if (packet.ForceRespawn)
        {
            CleanupExistingEntities(packet.Entities);
        }

        entitiesToSpawn.AddRange(packet.Entities);
        // TODO: Remove before merging
        foreach (Entity entity in packet.Entities)
        {
            if (!spawnCount.ContainsKey(entity.GetType()))
            {
                spawnCount[entity.GetType()] = 0;
            }
            spawnCount[entity.GetType()]++;
        }
    }

    public IEnumerator SpawnQueue()
    {
        while (true)
        {
            if (entitiesToSpawn.Count > 0 && !spawning)
            {
                spawning = true;
                List<Entity> list = new(entitiesToSpawn);
                entitiesToSpawn.Clear();
                yield return entities.SpawnBatchAsync(list).OnYieldError(Log.Error);
                spawning = false;
                // TODO: Remove before merging
                foreach (KeyValuePair<Type, int> entry in spawnCount)
                {
                    Log.Info($"{entry.Key.FullName}: {entry.Value}");
                }
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
