using System;
using Nitrox.Client.Communication.Abstract;

namespace Nitrox.Client.Communication.MultiplayerSession.ConnectionState
{
    public abstract class ConnectionNegotiatedState : CommunicatingState
    {
        public override void NegotiateReservation(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            throw new InvalidOperationException("Unable to negotiate a session connection in the current state.");
        }
    }
}
