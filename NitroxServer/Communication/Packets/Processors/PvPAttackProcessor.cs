using System.Collections.Generic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.Serialization;

namespace NitroxServer.Communication.Packets.Processors;

public class PvPAttackProcessor : AuthenticatedPacketProcessor<PvPAttack>
{
    private readonly ServerConfig serverConfig;
    private readonly PlayerManager playerManager;

    // TODO: In the future, do a whole config for damage sources
    private static readonly Dictionary<PvPAttack.AttackType, float> DamageMultiplierByType = new()
    {
        { PvPAttack.AttackType.KnifeHit, 0.5f },
        { PvPAttack.AttackType.HeatbladeHit, 1f }
    };

    public PvPAttackProcessor(ServerConfig serverConfig, PlayerManager playerManager)
    {
        this.serverConfig = serverConfig;
        this.playerManager = playerManager;
    }

    public override void Process(PvPAttack packet, Player player)
    {
        if (serverConfig.PvPEnabled &&
            playerManager.TryGetPlayerByName(packet.TargetPlayerName, out Player targetPlayer) &&
            DamageMultiplierByType.TryGetValue(packet.Type, out float multiplier))
        {
            packet.Damage *= multiplier;
            targetPlayer.SendPacket(packet);
        }
    }
}
