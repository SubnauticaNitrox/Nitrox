using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxClient.Communication.Abstract;
using NitroxClient.Debuggers;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Networking;
using NitroxModel.Packets;

namespace NitroxClient.Communication.NetworkingLayer.LiteNetLib
{
    public class LiteNetLibClient : IClient
    {
        public bool IsConnected { get; private set; }

        private readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
        private readonly AutoResetEvent connectedEvent = new AutoResetEvent(false);
        private readonly PacketReceiver packetReceiver;
        private readonly INetworkDebugger networkDebugger;

        private NetManager client;

        public LiteNetLibClient(PacketReceiver packetReceiver, INetworkDebugger networkDebugger = null)
        {
            this.packetReceiver = packetReceiver;
            this.networkDebugger = networkDebugger;
        }

        public void Start(string ipAddress, int serverPort)
        {
            Log.Info("Initializing LiteNetLibClient...");

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            netPacketProcessor.SubscribeReusable<WrapperPacket, NetPeer>(OnPacketReceived);

            EventBasedNetListener listener = new EventBasedNetListener();
            listener.PeerConnectedEvent += Connected;
            listener.PeerDisconnectedEvent += Disconnected;
            listener.NetworkReceiveEvent += ReceivedNetworkData;

            client = new NetManager(listener)
            {
                UpdateTime = 15,
                UnsyncedEvents = true,  //experimental feature, may need to replace with calls to client.PollEvents();
#if DEBUG
                DisconnectTimeout = 300000  //Disables Timeout (for 5 min) for debug purpose (like if you jump though the server code)
#endif
            };

            client.Start();
            client.Connect(ipAddress, serverPort, "nitrox");

            connectedEvent.WaitOne(2000);
            connectedEvent.Reset();
        }

        public void Send(Packet packet)
        {
            networkDebugger?.PacketSent(packet);
            client.SendToAll(netPacketProcessor.Write(packet.ToWrapperPacket()), NitroxDeliveryMethod.ToLiteNetLib(packet.DeliveryMethod));
            client.Flush();
        }

        public void Stop()
        {
            IsConnected = false;
            client.Stop();
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
            // IsConnected must happen before Set() so that its state is noticed WHEN we unblock the thread (cf. connectedEvent.WaitOne(...))
            IsConnected = true;
            connectedEvent.Set();
            Log.Info("Connected to server");
        }

        private void Disconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            // Check must happen before IsConnected is set to false, so that it doesn't send an exception when we aren't even ingame
            if (Multiplayer.Active)
            {
                Modal.Get<LostConnectionModal>()?.Show();
            }
            IsConnected = false;
            Log.Info("Disconnected from server");
        }
    }
}
