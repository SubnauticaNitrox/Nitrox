using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class AttackCyclopsTargetChangedProcessor(
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<AttackCyclopsTargetChanged>(entityRegistry)
{
    public override Task Process(AuthProcessorContext context, AttackCyclopsTargetChanged packet) => TransmitIfCanSeeEntities(packet, context.Sender.PlayerId, packet.CreatureId, packet.TargetId);
}
