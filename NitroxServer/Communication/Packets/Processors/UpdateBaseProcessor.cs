using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.Communication.Packets.Processors;

public class UpdateBaseProcessor : BuildingProcessor<UpdateBase>
{
    public UpdateBaseProcessor(BuildingManager buildingManager, PlayerManager playerManager) : base(buildingManager, playerManager) { }

    public override void Process(UpdateBase packet, Player player)
    {
        if (buildingManager.UpdateBase(player, packet, out int operationId))
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
