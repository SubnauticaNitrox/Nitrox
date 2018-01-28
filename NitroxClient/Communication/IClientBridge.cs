using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    public interface IClientBridge
    {
        ClientBridgeState CurrentState { get; }

        void connect(string ipAddress, string playerName);
        void disconnect();
        void send(Packet packet);
    }
}