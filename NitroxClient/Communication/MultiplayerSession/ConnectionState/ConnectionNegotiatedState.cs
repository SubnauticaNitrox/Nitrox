using System;
using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    public abstract class ConnectionNegotiatedState : CommunicatingState
    {
        public override Task NegotiateReservationAsync(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            throw new InvalidOperationException("Unable to negotiate a session connection in the current state.");
        }
    }
}
