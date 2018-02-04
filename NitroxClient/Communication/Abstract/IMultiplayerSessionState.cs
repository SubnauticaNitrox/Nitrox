using NitroxClient.Communication.MultiplayerSessionState;

namespace NitroxClient.Communication.Abstract
{
    public interface IMultiplayerSessionState
    {
        MultiplayerSessionConnectionStage ConnectionStage { get; }
        void Apply(IMultiplayerSessionManager sessionManager);
    }
}
