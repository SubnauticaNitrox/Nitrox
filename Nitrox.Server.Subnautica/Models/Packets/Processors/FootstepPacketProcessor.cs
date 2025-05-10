using System;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class FootstepPacketProcessor(FmodService fmodService) : IAuthPacketProcessor<FootstepPacket>
{
    private readonly FmodService fmodService = fmodService;
    private float footstepAudioRange; // To modify this value, modify the last value of the event:/player/footstep_precursor_base sound in the SoundWhitelist_Subnautica.csv file

    public async Task Process(AuthProcessorContext context, FootstepPacket packet)
    {
        // TODO: Load this inside FmodService?
        fmodService.TryGetSoundData("event:/player/footstep_precursor_base", out SoundData soundData);
        if (soundData == null)
        {
            throw new Exception("Missing audio data for footsteps");
        }
        footstepAudioRange = soundData.Radius;

        // TODO: USE DATABASE
        // foreach (NitroxServer.Player player in playerService.GetConnectedPlayersAsync())
        // {
        //     if (NitroxVector3.Distance(player.Position, sendingPlayer.Position) >= footstepAudioRange ||
        //         player == sendingPlayer)
        //     {
        //         continue;
        //     }
        //     if (player.SubRootId.Equals(sendingPlayer.SubRootId))
        //     {
        //         player.SendPacket(footstepPacket);
        //     }
        // }
    }
}
