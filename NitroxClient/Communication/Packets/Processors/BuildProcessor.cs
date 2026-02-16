using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic.Bases;

namespace NitroxClient.Communication.Packets.Processors;

internal abstract class BuildProcessor<T> : IClientPacketProcessor<T> where T : Packet
{
    public Task Process(ClientProcessorContext context, T packet)
    {
        BuildingHandler.Main.BuildQueue.Enqueue(packet);
        return Task.CompletedTask;
    }
}

internal class PlaceGhostProcessor : BuildProcessor<PlaceGhost>;

internal class PlaceModuleProcessor : BuildProcessor<PlaceModule>;

internal class ModifyConstructedAmountProcessor : BuildProcessor<ModifyConstructedAmount>;

internal class PlaceBaseProcessor : BuildProcessor<PlaceBase>;

internal class UpdateBaseProcessor : BuildProcessor<UpdateBase>;

internal class BaseDeconstructedProcessor : BuildProcessor<BaseDeconstructed>;

internal class PieceDeconstructedProcessor : BuildProcessor<PieceDeconstructed>;

internal class WaterParkDeconstructedProcessor : BuildProcessor<WaterParkDeconstructed>;

internal class LargeWaterParkDeconstructedProcessor : BuildProcessor<LargeWaterParkDeconstructed>;
