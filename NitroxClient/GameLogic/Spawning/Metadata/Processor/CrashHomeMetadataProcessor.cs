using System;
using System.Collections;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxClient.MonoBehaviours;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class CrashHomeMetadataProcessor : EntityMetadataProcessor<CrashHomeMetadata>
{
    private readonly Lazy<Entities> entities = new Lazy<Entities>(NitroxServiceLocator.LocateService<Entities>);

    public override void ProcessMetadata(GameObject gameObject, CrashHomeMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out CrashHome crashHome))
        {
            Log.Error($"[{nameof(CrashHomeMetadataProcessor)}] Could not find {nameof(CrashHome)} on {gameObject}");
            return;
        }

        crashHome.spawnTime = metadata.SpawnTime;

        if (!metadata.SpawnedCrashId.HasValue)
        {
            return;
        }

        if (NitroxEntity.TryGetComponentFrom(metadata.SpawnedCrashId.Value, out Crash crash))
        {
            crashHome.crash = crash;
            return;
        }

        if (entities.Value.SpawningEntities)
        {
            // Prevent spawn from occurring at all until we find out if the Crash is being spawned or not
            crashHome.spawnTime = float.MaxValue;
            CoroutineHost.StartCoroutine(DelayedPairToCrashAttempt(crashHome, metadata.SpawnedCrashId.Value));
        }
    }

    private IEnumerator DelayedPairToCrashAttempt(CrashHome crashHome, NitroxId crashId)
    {
        yield return new WaitUntil(() => !entities.Value.SpawningEntities);
        if (NitroxEntity.TryGetComponentFrom(crashId, out Crash crash))
        {
            crashHome.crash = crash;
        }
        // if no Crash was found, it might be because it has yet to be scheduled to spawn
        // if the Crash has spawned, the spawnTime is also -1
        crashHome.spawnTime = -1;
    }
}
