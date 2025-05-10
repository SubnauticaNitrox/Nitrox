using System.IO;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using NitroxModel.GameLogic.FMOD;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Audio service using the game audio assets.
/// </summary>
internal sealed class FmodService(GameInfo gameInfo, IOptions<ServerStartOptions> optionsProvider, ILogger<FmodService> logger) : IHostedService
{
    private readonly GameInfo gameInfo = gameInfo;
    private readonly IOptions<ServerStartOptions> optionsProvider = optionsProvider;
    private readonly ILogger<FmodService> logger = logger;
    private FmodWhitelist fmodWhitelist;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string filePath = Path.Combine(optionsProvider.Value.NitroxAssetsPath, "Resources", $"SoundWhitelist_{gameInfo.Name}.csv");
        string csvContent = await File.ReadAllTextAsync(filePath, cancellationToken);
        if (string.IsNullOrWhiteSpace(csvContent))
        {
            logger.LogError("Sound whitelist at '{FilePath}' is null or whitespace", filePath);
            return;
        }
        fmodWhitelist = FmodWhitelist.FromCsv(csvContent);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public bool TryGetSoundData(string path, out SoundData soundData) => fmodWhitelist.TryGetSoundData(path, out soundData);
}
