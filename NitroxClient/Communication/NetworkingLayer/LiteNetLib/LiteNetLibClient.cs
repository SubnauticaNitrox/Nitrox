using System.Net;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Core;
using NitroxModel.Logger;
using NitroxModel.Networking;
using NitroxModel.Packets;

namespace NitroxClient.Communication.NetworkingLayer.LiteNetLib
{
    public class LiteNetLibClient : IClient
    {
        public bool IsConnected { get; private set; }

        private readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
        private AutoResetEvent connectedEvent = new AutoResetEvent(false);
        private readonly PacketReceiver packetReceiver;
        private EventBasedNatPunchListener punchListener;
        private readonly string udpPunchServerAddress = "paschka.ddns.net";

        private NetManager client;

        public LiteNetLibClient()
        {
            packetReceiver = NitroxServiceLocator.LocateService<PacketReceiver>();
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

            client = new NetManager(listener);

            EventBasedNetListener listener2 = new EventBasedNetListener();
            listener2.PeerConnectedEvent += peer =>
            {
                Log.Debug("punchlisterner: connected to {0}", peer.EndPoint);
            };

            listener2.PeerDisconnectedEvent += (peer,reason) =>
            {
                Log.Debug("punchlisterner: disconnected from {0}", peer.EndPoint);
            };
            listener2.NetworkReceiveEvent += (peer, data, deliveryMethod) =>
            {
                EndPoint endPoint = data.GetNetEndPoint();
                Log.Debug("Revieved endpoint {0} from puncher. try to connect...", endPoint);
                client.Connect((IPEndPoint)endPoint, "nitrox");
            };
            NetManager punchClient = new NetManager(listener2);
            punchClient.UnsyncedEvents = true;
            //punchClient.Start();
            //punchClient.Connect("paschka.ddns.net", 11001, "NitroxPunch");
            

            punchListener = new EventBasedNatPunchListener();
            punchListener.NatIntroductionSuccess += (point, token) =>
            {
                Log.Debug("Got nat introduction to {1}. Will connect: {0}", !IsConnected, point);
                if (!IsConnected)
                {
                    //connectedEvent.Set();
                    client.Connect(point, "nitrox");
                }
            };

            
            client.NatPunchEnabled = true;
            client.NatPunchModule.Init(punchListener);

            client.UpdateTime = 15;
            client.UnsyncedEvents = true; //experimental feature, may need to replace with calls to client.PollEvents();
            client.Start();
            //client.Connect(ipAddress, serverPort, "nitrox");
            var endpoint = NetUtils.MakeEndPoint(udpPunchServerAddress, 11001);
            client.NatPunchModule.SendNatIntroduceRequest(endpoint, ipAddress);
            Log.Debug("Try to connect via hole punch to {0}", ipAddress);
            int rounds = 0;
            while(rounds < 25 && !connectedEvent.WaitOne(200))
            {                
                rounds++;
                client.NatPunchModule.PollEvents();                
            }
            Log.Debug("Rounds {0}", rounds);
            Thread.Sleep(100);
            //connectedEvent.WaitOne(2000);
            connectedEvent.Reset();
        }

        public void Send(Packet packet)
        {
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
            connectedEvent.Set();
            IsConnected = true;
            Log.Info("Connected to server {0}", peer.EndPoint);
        }

        private void Disconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            LostConnectionModal.Instance.Show();
            IsConnected = false;
            Log.Info("Disconnected from server {0}", peer.EndPoint);
        }
    }
}
