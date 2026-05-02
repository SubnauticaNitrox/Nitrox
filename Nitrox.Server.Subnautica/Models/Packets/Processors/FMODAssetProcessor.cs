using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class FMODAssetProcessor(PlayerManager playerManager, FmodService fmodService, ILogger<FMODAssetProcessor> logger) : IAuthPacketProcessor<FMODAssetPacket>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly FmodService fmodService = fmodService;
    private readonly ILogger<FMODAssetProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, FMODAssetPacket packet)
    {
        if (!fmodService.TryGetSoundData(packet.AssetPath, out SoundData soundData))
        {
            logger.ZLogError($"whitelist has no item for '{packet.AssetPath}'.");
            return;
        }

        foreach (Player player in playerManager.GetConnectedPlayers())
        {
            float distance = NitroxVector3.Distance(player.Position, packet.Position);
            if (player != context.Sender && (soundData.IsGlobal || player.SubRootId.Equals(context.Sender.SubRootId)) && distance <= soundData.Radius)
            {
                packet.Volume = SoundHelper.CalculateVolume(distance, soundData.Radius, packet.Volume);
                await context.SendAsync(packet, player.SessionId);
            }
        }
    }
}
