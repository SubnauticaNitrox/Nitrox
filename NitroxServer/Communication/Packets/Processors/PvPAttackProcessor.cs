using System.Collections.Generic;
using NitroxModel.Packets;
using NitroxModel.Serialization;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.Serialization;

namespace NitroxServer.Communication.Packets.Processors;

public class PvPAttackProcessor : AuthenticatedPacketProcessor<PvPAttack>
{
    private readonly SubnauticaServerConfig serverConfig;
    private readonly PlayerManager playerManager;

    // TODO: In the future, do a whole config for damage sources
    private static readonly Dictionary<PvPAttack.AttackType, float> damageMultiplierByType = new()
    {
        { PvPAttack.AttackType.KnifeHit, 0.5f },
        { PvPAttack.AttackType.HeatbladeHit, 1f }
    };

    public PvPAttackProcessor(SubnauticaServerConfig serverConfig, PlayerManager playerManager)
    {
        this.serverConfig = serverConfig;
        this.playerManager = playerManager;
    }

    public override void Process(PvPAttack packet, Player player)
    {
        if (!serverConfig.PvPEnabled)
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
