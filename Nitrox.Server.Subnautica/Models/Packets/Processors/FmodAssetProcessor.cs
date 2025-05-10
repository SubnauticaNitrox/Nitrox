using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Dto;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class FmodAssetProcessor(PlayerRepository playerRepository, FmodService fmodService, ILogger<FmodAssetProcessor> logger) : IAuthPacketProcessor<FMODAssetPacket>
{
    private readonly PlayerRepository playerRepository = playerRepository;
    private readonly FmodService fmodService = fmodService;
    private readonly ILogger<FmodAssetProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, FMODAssetPacket packet)
    {
        if (!fmodService.TryGetSoundData(packet.AssetPath, out SoundData soundData))
        {
            logger.ZLogError($"whitelist has no item for {packet.AssetPath:@Path}");
            return;
        }

        foreach (ConnectedPlayerDto player in await playerRepository.GetConnectedPlayersAsync())
        {
            // TODO: USE DATABASE
            // float distance = NitroxVector3.Distance(player.Position, packet.Position);
            // if (player != sendingPlayer && (soundData.IsGlobal || player.SubRootId.Equals(sendingPlayer.SubRootId)) && distance <= soundData.Radius)
            // {
            //     packet.Volume = SoundHelper.CalculateVolume(distance, soundData.Radius, packet.Volume);
            //     player.SendPacket(packet);
            // }
        }
    }
}
