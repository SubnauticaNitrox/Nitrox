using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;

namespace Nitrox.Client.Communication.Packets.Processors
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
                    .UpdatePosition(movement.Position.ToUnity(),
                        movement.Velocity.ToUnity(),
                        movement.BodyRotation.ToUnity(),
                        movement.AimingRotation.ToUnity());
            }
        }
    }
}
