using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

/// <summary>
/// Serializes Scanner Room initiated cold loads. BatchEntitySpawner currently owns
/// shared random state, so widening this semaphore requires making that spawner re-entrant first.
/// </summary>
internal sealed class ScannerRoomBatchLoader(WorldEntityManager worldEntityManager) : IScannerRoomBatchLoader
{
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly SemaphoreSlim loadGate = new(1, 1);

    public async Task LoadAsync(IReadOnlyList<NitroxInt3> batchIds, CancellationToken cancellationToken)
    {
        foreach (NitroxInt3 batchId in batchIds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await loadGate.WaitAsync(cancellationToken);
            try
            {
                await worldEntityManager.LoadUnspawnedEntitiesAsync(batchId, true);
            }
            finally
            {
                loadGate.Release();
            }
        }
    }
}
