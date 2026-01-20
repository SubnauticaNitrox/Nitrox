using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerMovementProcessor(IPacketSender packetSender, EntityRegistry entityRegistry) : AuthenticatedPacketProcessor<PlayerMovement>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public override void Process(PlayerMovement packet, Player player)
    {
        Optional<PlayerEntity> playerEntity = entityRegistry.GetEntityById<PlayerEntity>(player.PlayerContext.PlayerNitroxId);

        if (playerEntity.HasValue)
        {
            playerEntity.Value.Transform.Position = packet.Position;
            playerEntity.Value.Transform.Rotation = packet.BodyRotation;
        }

        player.Position = packet.Position;
        player.Rotation = packet.BodyRotation;
        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}
