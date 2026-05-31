#if DEBUG
using System.Collections.Generic;
using System.ComponentModel;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.ADMIN)]
internal sealed class LoadBatchCommand(BatchEntitySpawner batchEntitySpawner) : ICommandHandler<int, int, int>
{
    private readonly BatchEntitySpawner batchEntitySpawner = batchEntitySpawner;

    [Description("Loads entities at x y z")]
    public async Task Execute(ICommandContext context, int xCoordinate, int yCoordinate, int zCoordinate)
    {
        NitroxInt3 batchId = new(xCoordinate, yCoordinate, zCoordinate);
        List<Entity> entities = await batchEntitySpawner.LoadUnspawnedEntitiesAsync(batchId);

        await context.ReplyAsync($"Loaded {entities.Count} entities from batch {batchId}");
    }
}
#endif
