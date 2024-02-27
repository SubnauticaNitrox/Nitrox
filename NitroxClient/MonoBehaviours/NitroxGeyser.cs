using NitroxModel.DataStructures.GameLogic.Entities;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

/// <summary>
///     <see cref="Geyser"/> works with <see cref="MonoBehaviour.InvokeRepeating"/> which is based on <see cref="Time"/>.
///     Therefore, to ensure client freeze (and other things modifying the local unity's time) don't disturb the precise geyser's erupt schedule,
///     we manage it ourselves in a synced way.
/// </summary>
public class NitroxGeyser : MonoBehaviour
{
    private Geyser geyser;

    private float lastEruptTime;
    private float eruptInterval;

    public void Initialize(GeyserWorldEntity geyserEntity, Geyser geyser)
    {
        if (!DayNightCycle.main)
        {
            Log.Error($"Can't initialize {nameof(NitroxGeyser)} without {nameof(DayNightCycle)} being initialized");
            Destroy(this);
            return;
        }
        this.geyser = geyser;

        eruptInterval = geyser.eruptionInterval + geyserEntity.RandomIntervalVarianceMultiplier * geyser.eruptionIntervalVariance;
        float timePassed = DayNightCycle.main.timePassedAsFloat;
        float timeSinceLastErupt = (timePassed - geyserEntity.StartEruptTime) % eruptInterval;
        lastEruptTime = timePassed - timeSinceLastErupt;

        geyser.CancelInvoke(nameof(Geyser.Erupt));
    }

    public void Update()
    {
        if (!DayNightCycle.main)
        {
            return;
        }

        int eruptOccurrences = (int)((DayNightCycle.main.timePassedAsFloat - lastEruptTime) / eruptInterval);
        if (eruptOccurrences > 0)
        {
            lastEruptTime += eruptOccurrences * eruptInterval;
            if (geyser.erupting)
            {
                geyser.CancelInvoke(nameof(Geyser.EndErupt));
                geyser.EndErupt();
            }
            geyser.Erupt();
        }
    }
}
