using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class AnimationChangeEventProcessor : ClientPacketProcessor<AnimationChangeEvent>
{
    private readonly PlayerManager remotePlayerManager;

    public AnimationChangeEventProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;
    }

    public override void Process(AnimationChangeEvent animEvent)
    {
        Optional<RemotePlayer> opPlayer = remotePlayerManager.Find(animEvent.PlayerId);
        if (opPlayer.HasValue)
        {
            PlayerAnimation playerAnimation = animEvent.Animation;
            opPlayer.Value.UpdateAnimationAndCollider((AnimChangeType)playerAnimation.Type, (AnimChangeState)playerAnimation.State);
        }
    }
}
