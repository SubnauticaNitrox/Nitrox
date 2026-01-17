using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class FootstepPacketProcessor : AuthenticatedPacketProcessor<FootstepPacket>
{
    private float footstepAudioRange; // To modify this value, modify the last value of the event:/player/footstep_precursor_base sound in the SoundWhitelist_Subnautica.csv file
    private readonly PlayerManager playerManager;
    private readonly FmodService fmodService;

    public FootstepPacketProcessor(PlayerManager playerManager, FmodService fmodService)
    {
        this.playerManager = playerManager;
        this.fmodService = fmodService;
    }

    public override void Process(FootstepPacket footstepPacket, Player sendingPlayer)
    {
        if (footstepAudioRange == 0f && fmodService.TryGetSoundData("event:/player/footstep_precursor_base", out SoundData soundData))
        {
            footstepAudioRange = soundData.Radius;
        }

        foreach (Player player in playerManager.GetConnectedPlayers())
        {
            if (NitroxVector3.Distance(player.Position, sendingPlayer.Position) >= footstepAudioRange ||
                player == sendingPlayer)
            {
                continue;
            }
            if(player.SubRootId.Equals(sendingPlayer.SubRootId))
            {
                player.SendPacket(footstepPacket);
            }
        }
    }
}
