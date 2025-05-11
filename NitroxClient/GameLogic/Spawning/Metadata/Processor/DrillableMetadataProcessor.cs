using System.Linq;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxClient.Unity.Helper;
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

        // Updates health of each node and spawns VFX as if each node had been drilled
        // by the packet's given amounts in the order they are stored. See Drillable.OnDrill

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

                // Only use of Drillable.onDrilled, saves having to use reflection to invoke it
                if (drillable.TryGetComponentInParent(out AnteChamber antechamber))
                {
                    antechamber.OnDrilled(drillable);
                }
            }
        }
    }
}
