using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class RepairedComponentMetadataProcessor : GenericEntityMetadataProcessor<RepairedComponentMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, RepairedComponentMetadata metadata)
    {
        Radio radio = gameObject.GetComponent<Radio>();

        if (radio)
        {
            radio.liveMixin.health = radio.liveMixin.maxHealth;
            radio.repairNotification.Play();
        }

        EscapePod pod = gameObject.GetComponent<EscapePod>();

        if (pod)
        {
            pod.liveMixin.health = pod.liveMixin.maxHealth;
            pod.animator.SetFloat("lifepod_damage", 1.0f);
            pod.fixPanelGoal.Trigger();
            pod.fixPanelPowerUp.Play();
        }
    }
}
