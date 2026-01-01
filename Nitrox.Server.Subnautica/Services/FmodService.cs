using Nitrox.Model.GameLogic.FMOD;

namespace Nitrox.Server.Subnautica.Services;

sealed class FmodService(GameInfo gameInfo) : IHostedService
{
    private FMODWhitelist? whitelist;
    private readonly GameInfo gameInfo = gameInfo;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        whitelist = FMODWhitelist.Load(gameInfo);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public bool TryGetSoundData(string assetPath, out SoundData soundData)
    {
        if (whitelist == null)
        {
            soundData = default;
            return false;
        }
        return whitelist.TryGetSoundData(assetPath, out soundData);
    }
}
