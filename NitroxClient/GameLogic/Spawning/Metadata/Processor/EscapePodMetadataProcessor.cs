using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class EscapePodMetadataProcessor : EntityMetadataProcessor<EscapePodMetadata>
{
    // For metadata changes outside initial sync we only care about broken -> repaired
    public override void ProcessMetadata(GameObject gameObject, EscapePodMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out EscapePod pod))
        {
            Log.Error($"[{nameof(EscapePodMetadataProcessor)}] Could not get the EscapePod component from the provided gameobject.");
            return;
        }

        if (!pod.liveMixin.IsFullHealth() && metadata.PodRepaired)
        {
            pod.liveMixin.health = pod.liveMixin.maxHealth;
            pod.healthScalar = 1;
            pod.damageEffectsShowing = true;
            pod.UpdateDamagedEffects();
            pod.OnRepair();
        }

        if (!pod.radioSpawner.spawnedObj.TryGetComponent(out Radio radio))
        {
            Log.Error($"[{nameof(EscapePodMetadataProcessor)}] Could not get Radio from EscapePod.");
            return;
        }

        if (!radio.liveMixin.IsFullHealth() && metadata.RadioRepaired)
        {
            radio.liveMixin.AddHealth(radio.liveMixin.maxHealth);
        }
    }

    /// <summary>
    /// Applies repaired state without animations and minimal audio playback
    /// </summary>
    public static void ProcessInitialSyncMetadata(EscapePod pod, Radio radio, EscapePodMetadata metadata)
    {
        if (metadata.PodRepaired)
        {
            pod.liveMixin.health = pod.liveMixin.maxHealth;
            pod.healthScalar = 1;
            pod.damageEffectsShowing = true; // Needs to be set to true for UpdateDamagedEffects() to function
            pod.UpdateDamagedEffects();
            pod.vfxSpawner.SpawnManual(); // Spawn vfx to instantly disable it so no smoke is fading after player has joined
            pod.vfxSpawner.spawnedObj.SetActive(false);
            pod.lightingController.SnapToState(0);
        }
        else
        {
            IntroLifepodDirector introLifepodDirector = pod.GetComponent<IntroLifepodDirector>();
            introLifepodDirector.OnProtoDeserializeObjectTree(null);
            introLifepodDirector.ToggleActiveObjects(false);
            pod.lightingController.SnapToState(2);
        }

        if (metadata.RadioRepaired)
        {
            radio.liveMixin.health = radio.liveMixin.maxHealth;
            if (radio.liveMixin.loopingDamageEffectObj)
            {
                Object.Destroy(radio.liveMixin.loopingDamageEffectObj);
            }
        }
        else
        {
            pod.DamageRadio();
        }
    }
}
