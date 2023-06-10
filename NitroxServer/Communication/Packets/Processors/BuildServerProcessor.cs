using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors;

// TODO: Remove the serialized object before sending them back to the clients that should only be receiving lighter packets
public abstract class BuildingProcessor<T> : AuthenticatedPacketProcessor<T> where T : Packet
{
    internal readonly BuildingManager buildingManager;
    internal readonly PlayerManager playerManager;

    public BuildingProcessor(BuildingManager buildingManager, PlayerManager playerManager)
    {
        this.buildingManager = buildingManager;
        this.playerManager = playerManager;
    }

    public override void Process(T packet, Player player)
    {
        playerManager.SendPacketToOtherPlayers(packet, player);
    }

    public void ProcessWithOperationId(T packet, Player player, int operationId)
    {
        if (packet is BuildPacket buildPacket)
        {
            buildPacket.OperationId = operationId;
        }
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}

public class PlaceGhostProcessor : BuildingProcessor<PlaceGhost>
{
    public PlaceGhostProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(PlaceGhost packet, Player player)
    {
        if (buildingManager.AddGhost(packet))
        {
            base.Process(packet, player);
        }
    }
}

public class PlaceModuleProcessor : BuildingProcessor<PlaceModule>
{
    public PlaceModuleProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(PlaceModule packet, Player player)
    {
        if (buildingManager.AddModule(packet))
        {
            base.Process(packet, player);
        }
    }
}

public class ModifyConstructedAmountProcessor : BuildingProcessor<ModifyConstructedAmount>
{
    public ModifyConstructedAmountProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(ModifyConstructedAmount packet, Player player)
    {
        if (buildingManager.ModifyConstructedAmount(packet))
        {
            base.Process(packet, player);
        }
    }
}

public class PlaceBaseProcessor : BuildingProcessor<PlaceBase>
{
    public PlaceBaseProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(PlaceBase packet, Player player)
    {
        if (buildingManager.CreateBase(packet))
        {
            packet.BuildEntity = null;
            base.Process(packet, player);
        }
    }
}

public class UpdateBaseProcessor : BuildingProcessor<UpdateBase>
{
    public UpdateBaseProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(UpdateBase packet, Player player)
    {
        if (buildingManager.UpdateBase(packet, out int operationId))
        {
            packet.SavedBase = null;
            packet.BuiltPieceEntity = null;
            packet.UpdatedChildren = null;
            packet.UpdatedMoonpools = null;
            packet.UpdatedMapRooms = null;
            ProcessWithOperationId(packet, player, operationId);
        }
    }
}

public class BaseDeconstructedProcessor : BuildingProcessor<BaseDeconstructed>
{
    public BaseDeconstructedProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(BaseDeconstructed packet, Player player)
    {
        if (buildingManager.ReplaceBaseByGhost(packet))
        {
            base.Process(packet, player);
        }
    }
}

public class PieceDeconstructedProcessor : BuildingProcessor<PieceDeconstructed>
{
    public PieceDeconstructedProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(PieceDeconstructed packet, Player player)
    {
        if (buildingManager.ReplacePieceByGhost(packet, out _, out int operationId))
        {
            packet.SavedBase = null;
            ProcessWithOperationId(packet, player, operationId);
        }
    }
}

public class WaterParkDeconstructedProcessor : BuildingProcessor<WaterParkDeconstructed>
{
    public WaterParkDeconstructedProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(WaterParkDeconstructed packet, Player player)
    {
        if (buildingManager.ReplacePieceByGhost(packet, out Entity removedEntity, out int operationId) &&
            buildingManager.CreateWaterParkPiece(packet, removedEntity))
        {
            packet.SavedBase = null;
            ProcessWithOperationId(packet, player, operationId);
        }
    }
}
