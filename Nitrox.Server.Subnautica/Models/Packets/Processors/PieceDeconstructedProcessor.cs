using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PieceDeconstructedProcessor(GameLogic.Bases.BuildingManager buildingManager, PlayerRepository playerRepository, ILogger<PieceDeconstructedProcessor> logger) : IAuthPacketProcessor<PieceDeconstructed>
{
    private readonly GameLogic.Bases.BuildingManager buildingManager = buildingManager;
    private readonly PlayerRepository playerRepository = playerRepository;
    private readonly ILogger<PieceDeconstructedProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, PieceDeconstructed packet)
    {
        // TODO: Verify behavior is correct
        ConnectedPlayerDto connectedPlayer = await playerRepository.GetConnectedPlayerBySessionIdAsync(context.Sender.SessionId);
        if (connectedPlayer == null)
        {
            logger.ZLogWarning($"Lost connection with player id {context.Sender:@PlayerId}");
            packet.BaseData = null;
            packet.OperationId = -1;
            await context.ReplyToOthers(packet);
            return;
        }

        (Entity RemovedEntity, int OperationId) replacePieceByGhost = await buildingManager.ReplacePieceByGhost(connectedPlayer, packet);
        if (replacePieceByGhost.RemovedEntity != null)
        {
            packet.BaseData = null;
            packet.OperationId = replacePieceByGhost.OperationId;
            await context.ReplyToOthers(packet);
        }
    }
}
