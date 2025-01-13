using NitroxClient.Communication;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class EscapePodMetadataProcessor : EntityMetadataProcessor<EscapePodMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, EscapePodMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out EscapePod pod))
        {
            Log.Error($"[{nameof(EscapePodMetadataProcessor)}] Could not get the EscapePod component from the provided gameobject.");
            return;
        }

        if (!pod.radioSpawner.spawnedObj.TryGetComponent(out Radio radio))
        {
            Log.Error($"[{nameof(EscapePodMetadataProcessor)}] Could not get Radio from EscapePod.");
            return;
        }

        if (!pod.liveMixin.IsFullHealth() && metadata.PodRepaired)
        {
            pod.OnRepair();
        }

        if (!radio.liveMixin.IsFullHealth() && metadata.RadioRepaired)
        {
            radio.OnRepair();
        }

        ProcessInitialSyncMetadata(pod, radio, metadata);
    }

    public static void ProcessInitialSyncMetadata(EscapePod pod, Radio radio, EscapePodMetadata metadata)
    {
        using FMODSoundSuppressor soundSuppressor = FMODSystem.SuppressSubnauticaSounds();
        using PacketSuppressor<EntityMetadataUpdate> packetSuppressor = PacketSuppressor<EntityMetadataUpdate>.Suppress();

        if (metadata.PodRepaired)
        {
            pod.liveMixin.health = pod.liveMixin.maxHealth;
            pod.healthScalar = 1;
            pod.damageEffectsShowing = true;

            using (FMODSystem.SuppressSubnauticaSounds())
            {
                pod.UpdateDamagedEffects();
            }
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
            Object.Destroy(radio.liveMixin.loopingDamageEffectObj);
        }
        else
        {
            pod.DamageRadio();
        }
    }
}
