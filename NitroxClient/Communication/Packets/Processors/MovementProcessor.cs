using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class MovementProcessor : ClientPacketProcessor<Movement>
    {
        private readonly PlayerManager remotePlayerManager;

        public MovementProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(Movement movement)
        {
            Optional<RemotePlayer> remotePlayer = remotePlayerManager.Find(movement.PlayerId);

            if (remotePlayer.HasValue)
            {
                remotePlayer
                    .Value
                    .UpdatePosition(movement.Position,
                        movement.Velocity,
                        movement.BodyRotation,
                        movement.AimingRotation);
            }
        }
    }
}
