using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PvPAttackProcessor(IPacketSender packetSender, PlayerManager playerManager, IOptions<SubnauticaServerOptions> options) : AuthenticatedPacketProcessor<PvPAttack>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly PlayerManager playerManager = playerManager;

    // TODO: In the future, do a whole config for damage sources
    private static readonly Dictionary<PvPAttack.AttackType, float> damageMultiplierByType = new()
    {
        { PvPAttack.AttackType.KnifeHit, 0.5f },
        { PvPAttack.AttackType.HeatbladeHit, 1f }
    };

    public override void Process(PvPAttack packet, Player player)
    {
        if (!options.Value.PvpEnabled)
        {
            return;
        }
        if (!playerManager.TryGetPlayerBySessionId(packet.TargetPlayerId, out Player targetPlayer))
        {
            return;
        }
        if (!damageMultiplierByType.TryGetValue(packet.Type, out float multiplier))
        {
            return;
        }

        packet.Damage *= multiplier;
        packetSender.SendPacketAsync(packet, targetPlayer.SessionId);
    }
}
