using LiteNetLib;
using LiteNetLib.Utils;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System.Threading;

namespace NitroxClient.Communication
{
    public class UdpClient : IClient
    {
        private const int PORT = 11000;
        private const string CONNECTION_KEY = "nitrox";

        private readonly DeferringPacketReceiver packetReceiver;
        private NetManager client;
        private AutoResetEvent connectedEvent = new AutoResetEvent(false);

        public bool IsConnected { get; set; } = false;
        
        public UdpClient(DeferringPacketReceiver packetManager)
        {
            Log.Info("Initializing UdpClient...");
            packetReceiver = packetManager;
        }

        public void Start(string ipAddress)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            EventBasedNetListener listener = new EventBasedNetListener();
            listener.PeerConnectedEvent += Connected;
            listener.PeerDisconnectedEvent += Disconnected;
            listener.NetworkReceiveEvent += ReceivedNetworkData;
            
            client = new NetManager(listener, CONNECTION_KEY);
            client.UpdateTime = 15;
            client.UnsyncedEvents = true; //experimental feature, may need to replace with calls to client.PollEvents();
            client.Start();
            client.Connect(ipAddress, PORT);
            
            connectedEvent.WaitOne(2000);
            connectedEvent.Reset();
        }

        private void ReceivedNetworkData(NetPeer peer, NetDataReader reader)
        {
            if (reader.Data.Length > 0)
            {
                Packet packet = Packet.Deserialize(reader.Data);
                packetReceiver.PacketReceived(packet);
            }
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
            byte[] bytes = packet.Serialize();

            client.SendToAll(bytes, packet.DeliveryMethod);
            client.Flush();
        }

        public void Stop()
        {
            IsConnected = false;
            client.Stop();
        }
    }
}
