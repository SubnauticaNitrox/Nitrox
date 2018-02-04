using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSessionState;
using NitroxModel.Helper;

namespace NitroxClient.Communication
{
    public class MultiplayerSessionManager : IMultiplayerSessionManager
    {
        public event MultiplayerSessionManagerStateChangedEventHandler MultiplayerSessionManagerStateChanged;
        
        public IMultiplayerSessionState CurrentState { get; private set; }
        public IClient Client { get; }
        public string ServerIpAddress { get; }

        public MultiplayerSessionManager(IClient client)
        {
            Client = client;
            CurrentState = new DisconnectedMultiplayerSession();
        }

        internal void ChangeState(IMultiplayerSessionState newState)
        {
            Validate.NotNull(newState);
            if (MultiplayerSessionManagerStateChanged != null)
            {
                CurrentState = newState;
                MultiplayerSessionManagerStateChanged(newState);

                CurrentState.Apply(this);
            }
        }
    }
}
