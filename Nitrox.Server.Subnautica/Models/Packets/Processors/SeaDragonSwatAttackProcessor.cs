using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonSwatAttackProcessor(
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonSwatAttack>(entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, SeaDragonSwatAttack packet) => await TransmitIfCanSeeEntities(packet, context.Sender.PlayerId, packet.SeaDragonId, packet.TargetId);
}
