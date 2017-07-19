using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MovementProcessor : GenericPacketProcessor<Movement>
    {
        private PlayerGameObjectManager playerGameObjectManager;

        public MovementProcessor(PlayerGameObjectManager playerGameObjectManager)
        {
            this.playerGameObjectManager = playerGameObjectManager;
        }

        public override void Process(Movement movement)
        {
            playerGameObjectManager.UpdatePlayerPosition(movement.PlayerId, ApiHelper.Vector3(movement.PlayerPosition), ApiHelper.Quaternion(movement.Rotation), movement.SubGuid);
        }
    }
}
