using NitroxModel.DataStructures.Unity;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

public class FootstepPacketProcessor : AuthenticatedPacketProcessor<FootstepPacket>
{
    private readonly float footstepAudioRange; // To modify this value, modify the last value in the SoundWhitelist_Subnautica.csv file
    private readonly PlayerManager playerManager;

    public FootstepPacketProcessor(PlayerManager playerManager, FMODWhitelist whitelist)
    {
        this.playerManager = playerManager;
        whitelist.TryGetSoundData("event:/player/footstep_precursor_base", out SoundData soundData);
        footstepAudioRange = soundData.Radius;
    }

    public override void Process(FootstepPacket footstepPacket, Player sendingPlayer)
    {
        foreach (Player player in playerManager.GetAllPlayers())
        {
            if(NitroxVector3.Distance(player.Position, sendingPlayer.Position) >= footstepAudioRange)
            {
                continue;
            }
            if(player != sendingPlayer)
            {
                continue;
            }
            if (player.SubRootId.HasValue && sendingPlayer.SubRootId.HasValue &&
                player.SubRootId.Value == sendingPlayer.SubRootId.Value)
            {
                player.SendPacket(footstepPacket);
            }
            else if (NitroxVector3.Distance(player.Position, sendingPlayer.Position) <= footstepAudioRange &&
                    !player.SubRootId.HasValue)
            {
                player.SendPacket(footstepPacket);
            }
        }
    }
}
