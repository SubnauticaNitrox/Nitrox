using NitroxClient.GameLogic.Bases;
using NitroxModel.Networking.Packets;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public abstract class BuildProcessor<T> : IClientPacketProcessor<T> where T : Packet
{
    public Task Process(IPacketProcessContext context, T packet)
    {
        BuildingHandler.Main.BuildQueue.Enqueue(packet);
        return Task.CompletedTask;
    }
}

public class PlaceGhostProcessor : BuildProcessor<PlaceGhost> { }

public class PlaceModuleProcessor : BuildProcessor<PlaceModule> { }

public class ModifyConstructedAmountProcessor : BuildProcessor<ModifyConstructedAmount> { }

public class PlaceBaseProcessor : BuildProcessor<PlaceBase> { }

public class UpdateBaseProcessor : BuildProcessor<UpdateBase> { }

public class BaseDeconstructedProcessor : BuildProcessor<BaseDeconstructed> { }

public class PieceDeconstructedProcessor : BuildProcessor<PieceDeconstructed> { }

public class WaterParkDeconstructedProcessor : BuildProcessor<WaterParkDeconstructed> { }

public class LargeWaterParkDeconstructedProcessor : BuildProcessor<LargeWaterParkDeconstructed> { }
