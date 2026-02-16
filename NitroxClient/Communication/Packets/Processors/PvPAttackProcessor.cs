using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PvPAttackProcessor : IClientPacketProcessor<PvPAttack>
{
    public Task Process(ClientProcessorContext context, PvPAttack packet)
    {
        if (Player.main && Player.main.liveMixin)
        {
            Player.main.liveMixin.TakeDamage(packet.Damage);
        }
        return Task.CompletedTask;
    }
}
