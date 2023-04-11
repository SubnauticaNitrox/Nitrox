using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;

namespace NitroxServer.GameLogic.Entities.Spawning;

/// <summary>
/// When we operate in baked spawn mode there should never be unspawned batches.  This is an indicator that the Nitrox team
/// did not correctly load all of the batches in the vended prebaked file. This class will throw an error when encountering
/// an unhandled batch.
/// </summary>
public class NoOpBakedBatchEntitySpawner : BatchEntitySpawner
{
    public NoOpBakedBatchEntitySpawner(List<NitroxInt3> loadedPreviousParsed) : 
        base(null, null, null, loadedPreviousParsed, null, null, null, null)
    {

    }

    public override List<Entity> LoadUnspawnedEntities(NitroxInt3 batchId, bool fullCacheCreation = false)
    {
        Validate.IsTrue(IsBatchSpawned(batchId), $"Should not spawn entities when working in baked mode.  Unhandled batch: {batchId}");

        return new List<Entity>();
    }
}
