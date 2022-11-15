using System.Threading.Tasks;
using NitroxClient.Communication.MultiplayerSession;

namespace NitroxClient.Communication.Abstract
{
    public interface IMultiplayerSessionConnectionState
    {
        MultiplayerSessionConnectionStage CurrentStage { get; }
        
        Task NegotiateReservationAsync(IMultiplayerSessionConnectionContext sessionConnectionContext);
        void JoinSession(IMultiplayerSessionConnectionContext sessionConnectionContext);
        void Disconnect(IMultiplayerSessionConnectionContext sessionConnectionContext);
    }
}
