using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases.New;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

// TODO: Regroup every processor in only one
public class PlaceGhostProcessor : ClientPacketProcessor<PlaceGhost>
{
    public override void Process(PlaceGhost packet)
    {
        BuildingTester.Main.BuildQueue.Enqueue(packet);
    }
}

public class PlaceModuleProcessor : ClientPacketProcessor<PlaceModule>
{
    public override void Process(PlaceModule packet)
    {
        BuildingTester.Main.BuildQueue.Enqueue(packet);
    }
}

public class ModifyConstructedAmountProcessor : ClientPacketProcessor<ModifyConstructedAmount>
{
    public override void Process(ModifyConstructedAmount packet)
    {
        BuildingTester.Main.BuildQueue.Enqueue(packet);
    }
}

public class PlaceBaseProcessor : ClientPacketProcessor<PlaceBase>
{
    public override void Process(PlaceBase packet)
    {
        BuildingTester.Main.BuildQueue.Enqueue(packet);
    }
}

public class UpdateBaseProcessor : ClientPacketProcessor<UpdateBase>
{
    public override void Process(UpdateBase packet)
    {
        BuildingTester.Main.BuildQueue.Enqueue(packet);
    }
}

public class BaseDeconstructedProcessor : ClientPacketProcessor<BaseDeconstructed>
{
    public override void Process(BaseDeconstructed packet)
    {
        BuildingTester.Main.BuildQueue.Enqueue(packet);
    }
}

public class PieceDeconstructedProcessor : ClientPacketProcessor<PieceDeconstructed>
{
    public override void Process(PieceDeconstructed packet)
    {
        BuildingTester.Main.BuildQueue.Enqueue(packet);
    }
}
