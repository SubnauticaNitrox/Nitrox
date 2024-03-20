using System;
using NitroxClient.Communication.Abstract;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    public abstract class ConnectionNegotiatingState : CommunicatingState
    {
        public override void JoinSession(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            DisplayStatusCode(StatusCode.CONNECTION_FAIL_CLIENT, "Cannot join a session until a reservation has been negotiated with the server.");
        }
    }
}
