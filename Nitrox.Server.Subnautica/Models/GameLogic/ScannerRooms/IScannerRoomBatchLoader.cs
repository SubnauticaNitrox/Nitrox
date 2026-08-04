using System.Collections.Generic;
using Nitrox.Model.DataStructures;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

internal interface IScannerRoomBatchLoader
{
    Task LoadAsync(IReadOnlyList<NitroxInt3> batchIds, CancellationToken cancellationToken);
}
