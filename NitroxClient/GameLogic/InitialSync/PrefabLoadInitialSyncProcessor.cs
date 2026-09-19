using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.InitialSync;

/// <summary>
/// Ensures all prefabs are loaded before any spawning happens in-game. This allows to spawn prefabs without yielding which is a
/// huge plus when spawning an object that is related to a critical code path (in batteries code for example).
/// </summary>
internal sealed class PrefabLoadInitialSyncProcessor : InitialSyncProcessor
{
    private static bool loadStarted;
    private static readonly Queue<(string, TechType)> loadQueue = new(PrefabDatabase.prefabFiles.Keys.Count);
    private static readonly Stopwatch stopwatch = new();
    private static int workersRunning = 32;

    public PrefabLoadInitialSyncProcessor()
    {
        AddDependency<ClockSyncInitialSyncProcessor>();

        AddStep(WaitForAllPrefabLoaded);
    }

    public static IEnumerator WaitForAllPrefabLoaded(InitialPlayerSync packet)
    {
        yield return new WaitUntil(() => workersRunning == 0);
    }

    public static IEnumerator StartAllPrefabLoad()
    {
        if (loadStarted)
        {
            yield break;
        }
        loadStarted = true;

        CraftData.PreparePrefabIDCache();
        CraftData.PrepareEntTechCache();

        Log.Info($"Enqueuing loading for {PrefabDatabase.prefabFiles.Keys.Count} prefabs on {workersRunning} workers");

        stopwatch.Start();
        foreach (string classId in PrefabDatabase.prefabFiles.Keys)
        {
            if (CraftData.entClassTechTable.TryGetValue(classId, out TechType techType))
            {
                loadQueue.Enqueue((classId, techType));
            }
        }

        for (int i = 0; i < workersRunning; i++)
        {
            CoroutineHost.StartCoroutine(ProcessQueueWorker(loadQueue));
        }

        yield return new WaitUntil(() => workersRunning == 0);

        stopwatch.Stop();
        Log.Info($"All prefab loading took {(double)stopwatch.ElapsedTicks / Stopwatch.Frequency}s");
    }

    private static IEnumerator ProcessQueueWorker(Queue<(string classId, TechType techType)> queue)
    {
        while (queue.Count > 0)
        {
            (string classId, TechType techType) = queue.Dequeue();
            yield return DefaultWorldEntitySpawner.CachePrefab(classId, techType);
        }
        workersRunning--;
    }
}
