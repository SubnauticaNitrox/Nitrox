namespace NitroxClient.Communication.Abstract
{
    public interface IMultiplayerSessionConnectionState
    {
        MultiplayerSessionConnectionStage CurrentStage { get; }
        void HandleContext(IMultiplayerSessionConnectionContext sessionConnectionContext);
    }
}
