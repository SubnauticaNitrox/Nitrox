using System;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    public class JoinQueueEnqueued : ConnectionNegotiatedState
    {
        public override MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.JOIN_QUEUE_ENQUEUED;

        public override void JoinSession(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            throw new InvalidOperationException("Waiting in join queue.");
        }
    }
}
