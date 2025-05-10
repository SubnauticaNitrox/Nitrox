using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class AnimationProcessor : IClientPacketProcessor<AnimationChangeEvent>
    {
        private readonly PlayerManager remotePlayerManager;

        public AnimationProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public Task Process(IPacketProcessContext context, AnimationChangeEvent animEvent)
        {
            Optional<RemotePlayer> opPlayer = remotePlayerManager.Find(animEvent.PlayerId);
            if (opPlayer.HasValue)
            {
                opPlayer.Value.UpdateAnimationAndCollider((AnimChangeType)animEvent.Type, (AnimChangeState)animEvent.State);
            }
            return Task.CompletedTask;
        }
    }
}
