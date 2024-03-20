using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;
using NitroxModel;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    public abstract class CommunicatingState : IMultiplayerSessionConnectionState
    {
        public abstract MultiplayerSessionConnectionStage CurrentStage { get; }
        public abstract Task NegotiateReservationAsync(IMultiplayerSessionConnectionContext sessionConnectionContext);

        public abstract void JoinSession(IMultiplayerSessionConnectionContext sessionConnectionContext);

        public virtual void Disconnect(IMultiplayerSessionConnectionContext sessionConnectionContext)
        {
            sessionConnectionContext.ClearSessionState();
            sessionConnectionContext.Client.Stop();
            DisplayStatusCode(StatusCode.CONNECTION_FAIL_CLIENT, "Client was disconnected.");
            Disconnected newConnectionState = new Disconnected();
            sessionConnectionContext.UpdateConnectionState(newConnectionState);
        }
    }
}
