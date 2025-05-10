using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlaceBaseProcessor(BuildingManager buildingManager, EntitySimulation entitySimulation) : IAuthPacketProcessor<PlaceBase>
{
    private readonly BuildingManager buildingManager = buildingManager;
    private readonly EntitySimulation entitySimulation = entitySimulation;

    public async Task Process(AuthProcessorContext context, PlaceBase packet)
    {
        if (buildingManager.CreateBase(packet))
        {
            await entitySimulation.ClaimBuildPiece(packet.BuildEntity, context.Sender.PlayerId);
            packet.Deflate();
            await context.ReplyToOthers(packet);
        }
    }
}
