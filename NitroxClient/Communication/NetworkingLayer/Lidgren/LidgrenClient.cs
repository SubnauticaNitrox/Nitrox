using System.Threading;
using Lidgren.Network;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Core;
using NitroxModel.Logger;
using NitroxModel.Networking;
using NitroxModel.Packets;

namespace NitroxClient.Communication.NetworkingLayer.Lidgren
{
    public class LidgrenClient : IClient
    {
        public bool IsConnected { get; private set; }

        private NetClient client;
        private AutoResetEvent connectedEvent = new AutoResetEvent(false);
        private readonly DeferringPacketReceiver packetReceiver;

        public LidgrenClient()
        {
            packetReceiver = NitroxServiceLocator.LocateService<DeferringPacketReceiver>();
        }

        public void Start(string ipAddress, int serverPort)
        {
            Log.Info("Initializing LidgrenClient...");

            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            NetPeerConfiguration config = new NetPeerConfiguration("Nitrox");
            config.AutoFlushSendQueue = true;
            client = new NetClient(config);
            client.RegisterReceivedCallback(ReceivedMessage);
            client.Start();
            client.Connect(ipAddress, serverPort);
            connectedEvent.WaitOne(2000);
            connectedEvent.Reset();
        }

        public void Send(Packet packet)
        {
            byte[] bytes = packet.Serialize();

            NetOutgoingMessage om = client.CreateMessage();
            om.Write(bytes);

            client.SendMessage(om, NitroxDeliveryMethod.ToLidgren(packet.DeliveryMethod), (int)packet.UdpChannel);
            client.FlushSendQueue();
        }

        public void Stop()
        {
            client.UnregisterReceivedCallback(ReceivedMessage);
            client.Disconnect("");
        }

        public void ReceivedMessage(object peer)
        {
            NetIncomingMessage im;
            while ((im = client.ReadMessage()) != null)
            {
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        string text = im.ReadString();
                        Log.Info("Network message: " + text);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();
                        IsConnected = status == NetConnectionStatus.Connected;

                        if (IsConnected)
                        {
                            connectedEvent.Set();
                        }
                        else if (LostConnectionModal.Instance != null)
                        {
                            LostConnectionModal.Instance.Show();
                        }

                        Log.Info("IsConnected status: " + IsConnected);
                        break;
                    case NetIncomingMessageType.Data:
                        if (im.Data.Length > 0)
                        {
                            Packet packet = Packet.Deserialize(im.Data);
                            packetReceiver.PacketReceived(packet);
                        }

                        break;
                    default:
                        Log.Info("Unhandled lidgren message type: " + im.MessageType + " " + im.LengthBytes + " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);
                        break;
                }

                client.Recycle(im);
            }
        }
    }
}
