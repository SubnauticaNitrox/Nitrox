using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxClient.Communication.MultiplayerSession.ConnectionState;

namespace NitroxClient.Communication
{
    public class UdpClient : IClient
    {
        private readonly DeferringPacketReceiver packetReceiver;
        private AutoResetEvent connectedEvent = new AutoResetEvent(false);

        private const string CONNECTION_KEY = "nitrox";
        private NetManager client;
        private NetPacketProcessor netPacketProcessor = new NetPacketProcessor();

        public bool IsConnected { get; set; } = false;

        public UdpClient(DeferringPacketReceiver packetManager)
        {
            Log.Info("Initializing UdpClient...");
            packetReceiver = packetManager;
        }

        public void Start(string ipAddress, int serverPort)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            netPacketProcessor.SubscribeReusable<WrapperPacket, NetPeer>(OnPacketReceived);

            EventBasedNetListener listener = new EventBasedNetListener();
            listener.PeerConnectedEvent += Connected;
            listener.PeerDisconnectedEvent += Disconnected;
            listener.NetworkReceiveEvent += ReceivedNetworkData;

            client = new NetManager(listener);
            client.UpdateTime = 15;
            client.UnsyncedEvents = true; //experimental feature, may need to replace with calls to client.PollEvents();
            client.Start();
            client.Connect(ipAddress, serverPort, "nitrox");

            connectedEvent.WaitOne(2000);
            connectedEvent.Reset();
        }

        private void ReceivedNetworkData(NetPeer peer, NetDataReader reader, DeliveryMethod deliveryMethod)
        {
            netPacketProcessor.ReadAllPackets(reader, peer);
        }

        private void OnPacketReceived(WrapperPacket wrapperPacket, NetPeer peer)
        {
            Packet packet = Packet.Deserialize(wrapperPacket.packetData);
            packetReceiver.PacketReceived(packet);
        }

        private void Connected(NetPeer peer)
        {
            connectedEvent.Set();
            IsConnected = true;
            Log.Info("Connected to server");
        }

        private void Disconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            LostConnectionModal.Instance.Show();
            IsConnected = false;
            Log.Info("Disconnected from server");
        }

        public void Send(Packet packet)
        {
            client.SendToAll(netPacketProcessor.Write(packet.toWrapperPacket()), packet.DeliveryMethod);
            client.Flush();
        }

        public void Stop()
        {
            IsConnected = false;
            client.Stop();
        }
    }
}
