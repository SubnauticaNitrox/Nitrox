using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PvpAttackProcessor(IOptions<SubnauticaServerOptions> configProvider) : IAuthPacketProcessor<PvpAttack>
{
    private readonly IOptions<SubnauticaServerOptions> configProvider = configProvider;

    // TODO: In the future, do a whole config for damage sources
    private static readonly Dictionary<PvpAttack.AttackType, float> damageMultiplierByType = new()
    {
        { PvpAttack.AttackType.KnifeHit, 0.5f },
        { PvpAttack.AttackType.HeatbladeHit, 1f }
    };

    public async Task Process(AuthProcessorContext context, PvpAttack packet)
    {
        if (!configProvider.Value.PvpEnabled)
        {
            return;
        }
        if (!damageMultiplierByType.TryGetValue(packet.Type, out float multiplier))
        {
            return;
        }

        packet.Damage *= multiplier;
        await context.Send(packet, packet.TargetSessionId);
    }
}
