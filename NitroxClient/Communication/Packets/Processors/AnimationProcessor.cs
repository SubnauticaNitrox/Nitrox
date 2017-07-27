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
            var opPlayer = remotePlayerManager.Find(animEvent.PlayerId);
            if (opPlayer.IsPresent())
            {
                opPlayer.Get().UpdateAnimation((AnimChangeType)animEvent.Type, (AnimChangeState)animEvent.State);
            }
        }
    }
}
