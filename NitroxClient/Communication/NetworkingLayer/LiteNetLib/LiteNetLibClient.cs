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

            


            
            
            

            punchListener = new EventBasedNatPunchListener();
            punchListener.NatIntroductionSuccess += (point, token) =>
            {
                Log.Debug("Got nat introduction. Will connect: {0}",new object[] { IsConnected });
                if (!IsConnected)
                {
                    connectedEvent.Set();
                    client.Connect(point, "nitrox");
                }
            };

            client = new NetManager(listener);
            client.NatPunchEnabled = true;
            client.NatPunchModule.Init(punchListener);

            client.UpdateTime = 15;
            client.UnsyncedEvents = true; //experimental feature, may need to replace with calls to client.PollEvents();
            client.Start();
            client.Connect(ipAddress, serverPort, "nitrox");
            client.NatPunchModule.SendNatIntroduceRequest(NetUtils.MakeEndPoint("ghaarg.ddns.net", 11001), ipAddress);
            Log.Debug("Try to connect via hole punch");
            int rounds = 0;
            while(rounds < 500 && !IsConnected && !connectedEvent.WaitOne(10))
            {                
                rounds++;
                client.NatPunchModule.PollEvents();
            }
            
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
            Log.Info("Connected to server");
        }

        private void Disconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            LostConnectionModal.Instance.Show();
            IsConnected = false;
            Log.Info("Disconnected from server");
        }
    }
}
