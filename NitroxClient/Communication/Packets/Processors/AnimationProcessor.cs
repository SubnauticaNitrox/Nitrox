using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class AnimationProcessor : ClientPacketProcessor<AnimationChangeEvent>
    {
        private PlayerManager remotePlayerManager;

        public AnimationProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(AnimationChangeEvent animEvent)
        {
            remotePlayerManager.ForPlayer(animEvent.PlayerId, p => p.UpdateAnimation((AnimChangeType)animEvent.Type, (AnimChangeState)animEvent.State));
        }
    }
}
