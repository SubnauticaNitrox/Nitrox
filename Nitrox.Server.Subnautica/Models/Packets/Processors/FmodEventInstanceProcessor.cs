using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class FmodEventInstanceProcessor(FmodService fmodService, ILogger<FmodEventInstanceProcessor> logger) : IAuthPacketProcessor<FMODEventInstancePacket>
{
    private readonly FmodService fmodService = fmodService;
    private readonly ILogger<FmodEventInstanceProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, FMODEventInstancePacket packet)
    {
        if (!fmodService.TryGetSoundData(packet.AssetPath, out SoundData soundData))
        {
            logger.ZLogError($"whitelist has no item for {packet.AssetPath}");
            return;
        }

        // TODO: USE DATABASE
        // foreach (NitroxServer.Player player in playerService.GetConnectedPlayersAsync())
        // {
        //     float distance = NitroxVector3.Distance(player.Position, packet.Position);
        //     if (player != sendingPlayer &&
        //         (soundData.IsGlobal || player.SubRootId.Equals(sendingPlayer.SubRootId)) &&
        //         distance < soundData.Radius)
        //     {
        //         packet.Volume = SoundHelper.CalculateVolume(distance, soundData.Radius, packet.Volume);
        //         player.SendPacket(packet);
        //     }
        // }
    }
}
