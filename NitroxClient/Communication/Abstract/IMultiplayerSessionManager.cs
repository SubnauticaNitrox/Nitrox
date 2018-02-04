namespace NitroxClient.Communication.Abstract
{
    public delegate void MultiplayerSessionManagerStateChangedEventHandler(IMultiplayerSessionState newState);

    public interface IMultiplayerSessionManager
    {
        IMultiplayerSessionState CurrentState { get; }
        IClient Client { get; }
        string ServerIpAddress { get; }

        event MultiplayerSessionManagerStateChangedEventHandler MultiplayerSessionManagerStateChanged;
    }
}
