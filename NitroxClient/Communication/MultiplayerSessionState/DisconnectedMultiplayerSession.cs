using NitroxClient.Communication.Abstract;

namespace NitroxClient.Communication.MultiplayerSessionState
{
    internal class DisconnectedMultiplayerSession : IMultiplayerSessionState
    {
        public MultiplayerSessionConnectionStage ConnectionStage { get; }

        public void Apply(IMultiplayerSessionManager sessionManager)
        {
        }

        public DisconnectedMultiplayerSession()
        {
            ConnectionStage = MultiplayerSessionConnectionStage.Disconnected;
        }
    }
}
