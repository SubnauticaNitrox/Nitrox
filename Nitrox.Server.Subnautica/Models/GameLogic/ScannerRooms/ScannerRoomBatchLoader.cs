using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

/// <summary>
/// Limits Scanner Room batch-loading fan-out. BatchEntitySpawner independently serializes
/// every cold parse/spawn operation at the owner of its shared mutable state.
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
