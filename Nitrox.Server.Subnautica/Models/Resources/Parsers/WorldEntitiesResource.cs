using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.Resources.Core;
using WorldEntityInfo = UWE.WorldEntityInfo;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal sealed class WorldEntitiesResource(SubnauticaAssetsManager assetsManager, IOptions<ServerStartOptions> options) : IGameResource
{
    private readonly SubnauticaAssetsManager assetsManager = assetsManager;
    private readonly IOptions<ServerStartOptions> startOptions = options;
    private readonly TaskCompletionSource<Dictionary<string, WorldEntityInfo>> worldEntitiesByClassId = new();

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        worldEntitiesByClassId.TrySetResult(WorldEntityDataParser.Load(assetsManager, startOptions.Value, cancellationToken));
        return Task.CompletedTask;
    }

    public Task CleanupAsync()
    {
        assetsManager.Dispose();
        return Task.CompletedTask;
    }

    public Task<Dictionary<string, WorldEntityInfo>> GetWorldEntitiesByClassIdAsync() => worldEntitiesByClassId.Task;
}
