using Nitrox.Client.Communication.MultiplayerSession;

namespace Nitrox.Client.Communication.Abstract
{
    public interface IMultiplayerSessionConnectionState
    {
        MultiplayerSessionConnectionStage CurrentStage { get; }
        void NegotiateReservation(IMultiplayerSessionConnectionContext sessionConnectionContext);
        void JoinSession(IMultiplayerSessionConnectionContext sessionConnectionContext);
        void Disconnect(IMultiplayerSessionConnectionContext sessionConnectionContext);
    }
}
