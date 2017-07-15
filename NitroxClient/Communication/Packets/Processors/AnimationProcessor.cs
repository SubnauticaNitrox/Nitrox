using NitroxClient.Communication.Packets.Processors.Base;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxClient.Communication.Packets.Processors
{
    public class AnimationProcessor : GenericPacketProcessor<AnimationChangeEvent>
    {
        private PlayerGameObjectManager playerGameObjectManager;

        public AnimationProcessor(PlayerGameObjectManager playerGameObjectManager)
        {
            this.playerGameObjectManager = playerGameObjectManager;
        }

        public override void Process(AnimationChangeEvent animEvent)
        {
            playerGameObjectManager.UpdateAnimation(animEvent.PlayerId, (AnimChangeType)animEvent.Type, (AnimChangeState)animEvent.State);
        }
    }
}
