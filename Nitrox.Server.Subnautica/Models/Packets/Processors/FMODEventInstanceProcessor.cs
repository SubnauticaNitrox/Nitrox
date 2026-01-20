using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class FMODEventInstanceProcessor(IPacketSender packetSender, PlayerManager playerManager, FmodService fmodService, ILogger<FMODEventInstanceProcessor> logger) : AuthenticatedPacketProcessor<FMODEventInstancePacket>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly PlayerManager playerManager = playerManager;
    private readonly FmodService fmodService = fmodService;
    private readonly ILogger<FMODEventInstanceProcessor> logger = logger;

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
                packetSender.SendPacketAsync(packet, player.SessionId);
            }
        }
    }
}
