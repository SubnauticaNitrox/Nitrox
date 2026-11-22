using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class BaseFloodMetadataProcessor : EntityMetadataProcessor<BaseFloodMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, BaseFloodMetadata metadata)
    {
        BaseFloodSim baseFloodSim = gameObject.GetComponent<BaseFloodSim>();

        if (!baseFloodSim)
        {
            Log.Error($"Could not find BaseFloodSim on {gameObject.name}");
            return;
        }

        float[] incoming = metadata.FlatValueGrid;
        // The base's geometry may have changed (or not finished (re)building yet) since this snapshot was captured elsewhere,
        // in which case the array no longer maps to the same cells. Applying it regardless would throw inside BaseFloodSim's
        // own restore logic (index mismatch) and permanently break its Update() loop (no more leaks/flooding) from then on.
        if (incoming == null || incoming.Length != baseFloodSim.shape.Size)
        {
            return;
        }

        // Refresh flatValueGrid from the currently-simulated state so we can compare fairly before deciding to overwrite it.
        baseFloodSim.OnProtoSerialize(null);
        if (Sum(incoming) <= Sum(baseFloodSim.flatValueGrid))
        {
            // The local simulation is already at least as flooded as this (possibly network-delayed) snapshot. Skip applying it,
            // otherwise a client that's already actively simulating this base would keep getting its progress reset backwards
            // by other players' periodic updates. This still lets a freshly (re)joined client - whose own simulation starts
            // from empty - catch up to the real, more-flooded state.
            return;
        }

        baseFloodSim.flatValueGrid = incoming;
        baseFloodSim.OnProtoDeserialize(null);
    }

    private static float Sum(float[] values)
    {
        float total = 0f;
        foreach (float value in values)
        {
            total += value;
        }
        return total;
    }
}
