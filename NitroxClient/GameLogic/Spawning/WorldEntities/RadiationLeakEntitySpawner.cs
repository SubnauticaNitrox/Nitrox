using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class RadiationLeakEntitySpawner : SyncEntitySpawner<RadiationLeakEntity>
{
    // This constant is defined by Subnautica and should never be modified (same as for SubnauticaWorldModifier)
    private const int TOTAL_LEAKS = 11;
    private readonly TimeManager timeManager;
    private readonly List<float> registeredLeaksFixTime = new();

    public RadiationLeakEntitySpawner(TimeManager timeManager)
    {
        this.timeManager = timeManager;
    }

    protected override IEnumerator SpawnAsync(RadiationLeakEntity entity, TaskResult<Optional<GameObject>> result)
    {
        SpawnSync(entity, result);
        yield break;
    }

    protected override bool SpawnSync(RadiationLeakEntity entity, TaskResult<Optional<GameObject>> result)
    {
        // This script is located under (Aurora Scene) //Aurora-Main/Aurora so it's a good starting point to search through the GameObjects
        CrashedShipExploder crashedShipExploder = CrashedShipExploder.main;
        LeakingRadiation leakingRadiation = LeakingRadiation.main;
        if (!crashedShipExploder || !leakingRadiation || entity.Metadata is not RadiationMetadata metadata)
        {
            return true;
        }
        Transform radiationLeaksHolder = crashedShipExploder.transform.Find("radiationleaks").GetChild(0);
        RadiationLeak radiationLeak = radiationLeaksHolder.GetChild(entity.ObjectIndex).GetComponent<RadiationLeak>();
        NitroxEntity.SetNewId(radiationLeak.gameObject, entity.Id);
        radiationLeak.liveMixin.health = metadata.Health;
        registeredLeaksFixTime.Add(metadata.FixRealTime);

        // We can only calculate the radiation increment and dissipation once we got all radiation leaks info
        if (crashedShipExploder.IsExploded() && registeredLeaksFixTime.Count == TOTAL_LEAKS)
        {
            RecalculateRadiationRadius(leakingRadiation);
        }

        return true;
    }

    public void RecalculateRadiationRadius(LeakingRadiation leakingRadiation)
    {
        float realElapsedTime = (float)timeManager.RealTimeElapsed;
        // We substract the explosion time from the real time because before that, the radius doesn't increment
        float realExplosionTime = timeManager.AuroraRealExplosionTime;
        float maxRegisteredLeakFixTime = registeredLeaksFixTime.Max();

        // Note: Only increment radius if leaks were fixed AFTER explosion (before, game code doesn't increase radius)
        
        // If leaks aren't all fixed yet we calculate from current real elapsed time
        float deltaTimeAfterExplosion = realElapsedTime - realExplosionTime;
        if (maxRegisteredLeakFixTime == -1)
        {
            if (deltaTimeAfterExplosion > 0)
            {
                float radiusIncrement = deltaTimeAfterExplosion * leakingRadiation.kGrowRate;
                // Calculation lines from LeakingRadiation.Update
                leakingRadiation.currentRadius = Mathf.Clamp(leakingRadiation.kStartRadius + radiusIncrement, 0f, leakingRadiation.kMaxRadius);
                leakingRadiation.damagePlayerInRadius.damageRadius = leakingRadiation.currentRadius;
                leakingRadiation.radiatePlayerInRange.radiateRadius = leakingRadiation.currentRadius;
            }
            // If leaks aren't fixed, we won't need to calculate a radius decrement
            return;
        }
        leakingRadiation.radiationFixed = true;

        // If all leaks are fixed we calculate from the time they were fixed
        float deltaAliveTime = maxRegisteredLeakFixTime - realExplosionTime;
        if (deltaAliveTime > 0)
        {
            float radiusIncrement = deltaAliveTime * leakingRadiation.kGrowRate;
            leakingRadiation.currentRadius = Mathf.Clamp(leakingRadiation.kStartRadius + radiusIncrement, 0f, leakingRadiation.kMaxRadius);
        }

        // Now calculate the natural dissipation decrement from the time leaks are fixed
        // If they were fixed before real explosion time, we calculate from real explosion time
        float deltaFixedTimeAfterExplosion = realElapsedTime - Mathf.Max(maxRegisteredLeakFixTime, realExplosionTime);
        if (deltaFixedTimeAfterExplosion > 0)
        {
            float radiusDecrement = deltaFixedTimeAfterExplosion * leakingRadiation.kNaturalDissipation;
            leakingRadiation.currentRadius = Mathf.Clamp(leakingRadiation.currentRadius + radiusDecrement, 0f, leakingRadiation.kMaxRadius);
        }
        leakingRadiation.damagePlayerInRadius.damageRadius = leakingRadiation.currentRadius;
        leakingRadiation.radiatePlayerInRange.radiateRadius = leakingRadiation.currentRadius;
    }

    protected override bool SpawnsOwnChildren(RadiationLeakEntity entity) => false;
}
