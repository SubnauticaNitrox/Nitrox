using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class AnimationProcessor : ClientPacketProcessor<AnimationChangeEvent>
    {
        private readonly PlayerManager remotePlayerManager;

        public AnimationProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(AnimationChangeEvent animEvent)
        {
            Optional<RemotePlayer> opPlayer = remotePlayerManager.Find(animEvent.PlayerId);
            if (opPlayer.HasValue)
            {
                opPlayer.Value.UpdateAnimation((AnimChangeType)animEvent.Type, (AnimChangeState)animEvent.State);
            }
        }
    }
}
