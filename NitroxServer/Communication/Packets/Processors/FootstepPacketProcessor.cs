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
        var players = playerManager.GetAllPlayers();
        foreach (Player player in players)
        {
            if (sendingPlayer.SubRootId.HasValue)
            {
                if (player.SubRootId.HasValue)
                {
                    // If both players have id's, check if they are the same
                    if (NitroxVector3.Distance(player.Position, sendingPlayer.Position) <= footstepAudioRange && player != sendingPlayer && player.SubRootId.Value == sendingPlayer.SubRootId.Value)
                    {
                        // Forward footstep packet to players if they are within range to hear it and are in the same structure / submarine
                        player.SendPacket(footstepPacket);
                    }
                }
                // If one player has an id and the other doesn't, automatically false
            }
            else
            {
                // if both player's don't have SubRootIds
                if (!player.SubRootId.HasValue)
                {
                    if (NitroxVector3.Distance(player.Position, sendingPlayer.Position) <= footstepAudioRange && player != sendingPlayer)
                    {
                        // Forward footstep packet to players if they are within range to hear it and are in the same structure / submarine
                        player.SendPacket(footstepPacket);
                    }
                }
                // If one player doesn't have an id and other does, automatically false
            }
        }
    }
}
