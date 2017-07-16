using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MovementProcessor : ClientPacketProcessor<Movement>
    {
        private PlayerManager remotePlayerManager;

        public MovementProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(Movement movement)
        {
            remotePlayerManager.ForPlayer(movement.PlayerId, p => p.UpdatePosition(ApiHelper.Vector3(movement.PlayerPosition), ApiHelper.Quaternion(movement.BodyRotation), ApiHelper.Quaternion(movement.CameraRotation), movement.SubGuid), true);
        }
    }
}
