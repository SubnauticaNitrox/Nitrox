using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class FootstepPacketProcessor(PlayerService playerService, FmodService fmodService) : IAuthPacketProcessor<FootstepPacket>
{
    private readonly FmodService fmodService = fmodService;
    private readonly PlayerService playerService = playerService;
    private float footstepAudioRange; // To modify this value, modify the last value of the event:/player/footstep_precursor_base sound in the SoundWhitelist_Subnautica.csv file

    public async Task Process(AuthProcessorContext context, FootstepPacket footstepPacket)
    {
        if (footstepAudioRange == 0f && fmodService.TryGetSoundData("event:/player/footstep_precursor_base", out SoundData soundData))
        {
            footstepAudioRange = soundData.Radius;
        }

        foreach (SessionId player in playerService.GetSessionIds())
        {
            if (NitroxVector3.Distance(playerService.GetProperty<PositionProperty>(player).Value, playerService.GetProperty<PositionProperty>(context.Sender).Value) >= footstepAudioRange ||
                player == context.Sender)
            {
                continue;
            }
            if (playerService.GetProperty<SubRootIdProperty>(player).Value == playerService.GetProperty<SubRootIdProperty>(context.Sender).Value)
            {
                await context.SendAsync(footstepPacket, player);
            }
        }
    }
}
