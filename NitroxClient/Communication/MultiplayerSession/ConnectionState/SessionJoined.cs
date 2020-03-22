using System;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    public class SessionJoined : ConnectionNegotiatedState
    {
        public override MultiplayerSessionConnectionStage CurrentStage => MultiplayerSessionConnectionStage.SESSION_JOINED;

        public override void JoinSession(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            throw new InvalidOperationException("The session is already in progress.");
        }
    }
}
