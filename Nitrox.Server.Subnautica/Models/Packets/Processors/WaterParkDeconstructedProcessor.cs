using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class WaterParkDeconstructedProcessor(GameLogic.Bases.BuildingManager buildingManager, PlayerRepository playerRepository) : IAuthPacketProcessor<WaterParkDeconstructed>
{
    private readonly GameLogic.Bases.BuildingManager buildingManager = buildingManager;
    private readonly PlayerRepository playerRepository = playerRepository;

    public async Task Process(AuthProcessorContext context, WaterParkDeconstructed packet)
    {
        ConnectedPlayerDto player = await playerRepository.GetConnectedPlayerBySessionIdAsync(context.Sender.SessionId);

        (Entity removedEntity, int operationId) = await buildingManager.ReplacePieceByGhost(player, packet);
        if (removedEntity != null && buildingManager.CreateWaterParkPiece(packet, removedEntity))
        {
            packet.BaseData = null;
            packet.OperationId = operationId;
            await context.ReplyToOthers(packet);
        }
    }
}
