using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class FMODEventInstanceProcessor : AuthenticatedPacketProcessor<FMODEventInstancePacket>
{
    private readonly PlayerManager playerManager;
    private readonly FMODWhitelist fmodWhitelist;

    public FMODEventInstanceProcessor(PlayerManager playerManager, FMODWhitelist fmodWhitelist)
    {
        this.playerManager = playerManager;
        this.fmodWhitelist = fmodWhitelist;
    }

    public override void Process(FMODEventInstancePacket packet, Player sendingPlayer)
    {
        if (!fmodWhitelist.TryGetSoundData(packet.AssetPath, out SoundData soundData))
        {
            Log.Error($"[{nameof(FMODEventInstanceProcessor)}] Whitelist has no item for {packet.AssetPath}.");
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
