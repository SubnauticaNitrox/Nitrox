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
            // End-players can process elementary operations without this data (packet would be heavier for no reason)
            packet.Deflate();
            SendToOtherPlayersWithOperationId(packet, player, operationId);
        }
    }
}
