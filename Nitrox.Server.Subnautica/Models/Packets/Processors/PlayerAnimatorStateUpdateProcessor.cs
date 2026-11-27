using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.GameLogic.PlayerAnimation;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerAnimatorStateUpdateProcessor : IAuthPacketProcessor<PlayerAnimatorStateUpdate>
{
    public async Task Process(AuthProcessorContext context, PlayerAnimatorStateUpdate packet)
    {
        Dictionary<string, bool> synchronizedParameters = packet.AnimatorParameters
            .Where(parameter => PlayerAnimatorParameter.IsSynchronized(parameter.Key))
            .ToDictionary(parameter => parameter.Key, parameter => parameter.Value);
        if (synchronizedParameters.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<string, bool> parameter in synchronizedParameters)
        {
            context.Sender.PlayerContext.AnimatorParameters[parameter.Key] = parameter.Value;
        }

        await context.SendToOthersAsync(new PlayerAnimatorStateUpdate(context.Sender.PlayerContext.SessionId, synchronizedParameters));
    }
}
