using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class EntityReparentedProcessor(IPacketSender packetSender, EntityRegistry entityRegistry, ILogger<EntityReparentedProcessor> logger) : AuthenticatedPacketProcessor<EntityReparented>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<EntityReparentedProcessor> logger = logger;

    public override void Process(EntityReparented packet, Player player)
    {
        if (!entityRegistry.TryGetEntityById(packet.Id, out Entity entity))
        {
            logger.ZLogError($"Couldn't find entity for {packet.Id}");
            return;
        }
        if (!entityRegistry.TryGetEntityById(packet.NewParentId, out Entity parentEntity))
        {
            logger.ZLogError($"Couldn't find parent entity for {packet.NewParentId}");
            return;
        }

        entityRegistry.ReparentEntity(packet.Id, packet.NewParentId);
        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}
