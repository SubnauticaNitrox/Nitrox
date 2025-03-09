using System.Linq;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class DrillableMetadataProcessor : EntityMetadataProcessor<DrillableMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, DrillableMetadata metadata)
    {
        Drillable drillable = gameObject.GetComponent<Drillable>();
        if (!drillable)
        {
            Log.Error($"Could not find Drillable on {gameObject.name}");
            return;
        }

        drillable.timeLastDrilled = metadata.TimeLastDrilled;

        if (drillable.health == null)
        {
            drillable.health = metadata.ChunkHealth;
            return;
        }

        Validate.IsTrue(drillable.health.Length == metadata.ChunkHealth.Length);

        float totalHealth = drillable.health.Sum();

        for (int i = 0; i < drillable.health.Length; i++)
        {
            Vector3 chunkPos = UWE.Utils.GetEncapsulatedAABB(drillable.renderers[i].gameObject).center;

            if (drillable.health[i] > 0 && metadata.ChunkHealth[i] <= 0)
            {
                drillable.renderers[i].gameObject.SetActive(false);
                drillable.SpawnFX(drillable.breakFX, chunkPos);
            }
            else if (drillable.health[i] <= 0 && metadata.ChunkHealth[i] > 0)
            {
                drillable.renderers[i].gameObject.SetActive(true);
            }

            float oldTotalHealth = totalHealth;
            totalHealth -= drillable.health[i];
            totalHealth += metadata.ChunkHealth[i];

            drillable.health[i] = metadata.ChunkHealth[i];

            if (oldTotalHealth > 0 && totalHealth <= 0)
            {
                drillable.SpawnFX(drillable.breakAllFX, chunkPos);
            }
        }
    }
}
