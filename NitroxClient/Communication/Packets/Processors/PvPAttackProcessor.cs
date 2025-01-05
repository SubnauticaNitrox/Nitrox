using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PvPAttackProcessor : ClientPacketProcessor<PvPAttack>
{
    public override void Process(PvPAttack packet)
    {
        if (Player.main && Player.main.liveMixin)
        {
            Player.main.liveMixin.TakeDamage(packet.Damage);
        }
    }
}
