using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class RepairedComponentMetadataProcessor : EntityMetadataProcessor<RepairedComponentMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, RepairedComponentMetadata metadata)
    {
        if (gameObject.TryGetComponent(out Radio radio))
        {
            radio.liveMixin.health = radio.liveMixin.maxHealth;
            radio.repairNotification.Play();
        }

        if (gameObject.TryGetComponent(out EscapePod pod))
        {
            pod.liveMixin.health = pod.liveMixin.maxHealth;
            pod.animator.SetFloat("lifepod_damage", 1.0f);
            pod.fixPanelGoal.Trigger();
            pod.fixPanelPowerUp.Play();
        }
    }
}
