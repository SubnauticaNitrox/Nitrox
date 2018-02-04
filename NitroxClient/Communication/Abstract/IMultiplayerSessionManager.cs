namespace NitroxClient.Communication.Abstract
{
    public delegate void MultiplayerSessionManagerStateChangedEventHandler(IMultiplayerSessionState newState);

    public interface IMultiplayerSessionManager
    {
        IMultiplayerSessionState CurrentState { get; }

        event MultiplayerSessionManagerStateChangedEventHandler MultiplayerSessionManagerStateChanged;
    }
}
