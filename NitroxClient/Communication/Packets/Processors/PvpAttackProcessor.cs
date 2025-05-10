using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PvpAttackProcessor : IClientPacketProcessor<PvpAttack>
{
    public Task Process(IPacketProcessContext context, PvpAttack packet)
    {
        if (Player.main && Player.main.liveMixin)
        {
            Player.main.liveMixin.TakeDamage(packet.Damage);
        }

        return Task.CompletedTask;
    }
}
