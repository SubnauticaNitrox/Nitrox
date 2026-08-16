using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.Services;
using NitroxClient.Services.Multiplayer;

namespace NitroxClient.Communication.Packets.Processors;

internal abstract class BuildProcessor<T>(BuildingService buildingService) : IClientPacketProcessor<T> where T : Packet
{
    private readonly BuildingService buildingService = buildingService;

    public Task Process(ClientProcessorContext context, T packet)
    {
        buildingService.BuildQueue.Enqueue(packet);
        return Task.CompletedTask;
    }
}

internal class PlaceGhostProcessor(BuildingService buildingService) : BuildProcessor<PlaceGhost>(buildingService);

internal class PlaceModuleProcessor(BuildingService buildingService) : BuildProcessor<PlaceModule>(buildingService);

internal class ModifyConstructedAmountProcessor(BuildingService buildingService) : BuildProcessor<ModifyConstructedAmount>(buildingService);

internal class PlaceBaseProcessor(BuildingService buildingService) : BuildProcessor<PlaceBase>(buildingService);

internal class UpdateBaseProcessor(BuildingService buildingService) : BuildProcessor<UpdateBase>(buildingService);

internal class BaseDeconstructedProcessor(BuildingService buildingService) : BuildProcessor<BaseDeconstructed>(buildingService);

internal class PieceDeconstructedProcessor(BuildingService buildingService) : BuildProcessor<PieceDeconstructed>(buildingService);

internal class WaterParkDeconstructedProcessor(BuildingService buildingService) : BuildProcessor<WaterParkDeconstructed>(buildingService);

internal class LargeWaterParkDeconstructedProcessor(BuildingService buildingService) : BuildProcessor<LargeWaterParkDeconstructed>(buildingService);
