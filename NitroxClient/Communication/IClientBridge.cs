namespace NitroxClient.Communication
{
    public interface IClientBridge : IPacketSender
    {
        ClientBridgeState CurrentState { get; }

        void connect(string ipAddress, string playerName);
        void disconnect();
    }
}