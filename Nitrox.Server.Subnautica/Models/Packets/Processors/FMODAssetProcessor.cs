using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class FMODAssetProcessor(PlayerService playerService, FmodService fmodService, ILogger<FMODAssetProcessor> logger) : IAuthPacketProcessor<FMODAssetPacket>
{
    private readonly PlayerService playerService = playerService;
    private readonly FmodService fmodService = fmodService;
    private readonly ILogger<FMODAssetProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, FMODAssetPacket packet)
    {
        if (!fmodService.TryGetSoundData(packet.AssetPath, out SoundData soundData))
        {
            logger.ZLogError($"whitelist has no item for '{packet.AssetPath}'.");
            return;
        }

        foreach (SessionId player in playerService.GetSessionIds())
        {
            float distance = NitroxVector3.Distance(playerService.GetProperty<PositionProperty>(player).Value, packet.Position);
            if (player == context.Sender)
            {
                continue;
            }
            if (distance > soundData.Radius)
            {
                continue;
            }
            if (soundData.IsGlobal || playerService.GetProperty<SubRootIdProperty>(player).Value == playerService.GetProperty<SubRootIdProperty>(context.Sender).Value)
            {
                packet.Volume = SoundHelper.CalculateVolume(distance, soundData.Radius, packet.Volume);
                await context.SendAsync(packet, player);
            }
        }
    }
}
