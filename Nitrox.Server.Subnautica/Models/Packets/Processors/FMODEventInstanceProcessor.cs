using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class FMODEventInstanceProcessor : AuthenticatedPacketProcessor<FMODEventInstancePacket>
{
    private readonly PlayerManager playerManager;
    private readonly FmodService fmodService;
    private readonly ILogger<FMODEventInstanceProcessor> logger;

    public FMODEventInstanceProcessor(PlayerManager playerManager, FmodService fmodService, ILogger<FMODEventInstanceProcessor> logger)
    {
        this.playerManager = playerManager;
        this.fmodService = fmodService;
        this.logger = logger;
    }

    public override void Process(FMODEventInstancePacket packet, Player sendingPlayer)
    {
        if (!fmodService.TryGetSoundData(packet.AssetPath, out SoundData soundData))
        {
            logger.ZLogError($"Whitelist has no item for {packet.AssetPath}.");
            return;
        }

        foreach (Player player in playerManager.GetConnectedPlayers())
        {
            float distance = NitroxVector3.Distance(player.Position, packet.Position);
            if (player != sendingPlayer &&
                (soundData.IsGlobal || player.SubRootId.Equals(sendingPlayer.SubRootId)) &&
                distance < soundData.Radius)
            {
                packet.Volume = SoundHelper.CalculateVolume(distance, soundData.Radius, packet.Volume);
                player.SendPacket(packet);
            }
        }
    }
}
