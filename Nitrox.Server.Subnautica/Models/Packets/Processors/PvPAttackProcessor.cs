using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PvPAttackProcessor : AuthenticatedPacketProcessor<PvPAttack>
{
    private readonly IOptions<SubnauticaServerOptions> options;
    private readonly PlayerManager playerManager;

    // TODO: In the future, do a whole config for damage sources
    private static readonly Dictionary<PvPAttack.AttackType, float> damageMultiplierByType = new()
    {
        { PvPAttack.AttackType.KnifeHit, 0.5f },
        { PvPAttack.AttackType.HeatbladeHit, 1f }
    };

    public PvPAttackProcessor(IOptions<SubnauticaServerOptions> options, PlayerManager playerManager)
    {
        this.options = options;
        this.playerManager = playerManager;
    }

    public override void Process(PvPAttack packet, Player player)
    {
        if (!options.Value.PvpEnabled)
        {
            return;
        }
        if (!playerManager.TryGetPlayerById(packet.TargetPlayerId, out Player targetPlayer))
        {
            return;
        }
        if (!damageMultiplierByType.TryGetValue(packet.Type, out float multiplier))
        {
            return;
        }

        packet.Damage *= multiplier;
        targetPlayer.SendPacket(packet);
    }
}
