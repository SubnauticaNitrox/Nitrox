﻿using Lidgren.Network;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System.IO;
using System.Threading;

namespace NitroxClient.Communication
{
    public class UdpClient : IClient
    {
        private readonly DeferringPacketReceiver packetReceiver;
        private NetClient client;
        private AutoResetEvent connectedEvent = new AutoResetEvent(false);

        public bool IsConnected { get; set; } = false;
        
        public UdpClient(DeferringPacketReceiver packetManager)
        {
            Log.Info("Initializing UdpClient...");
            packetReceiver = packetManager;
        }

        public void Start(string ipAddress, int serverPort)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            
            NetPeerConfiguration config = new NetPeerConfiguration("Nitrox");
            config.AutoFlushSendQueue = true;
            client = new NetClient(config);
            client.RegisterReceivedCallback(new SendOrPostCallback(ReceivedMessage));
            client.Start();
            client.Connect(ipAddress, serverPort);
            connectedEvent.WaitOne(2000);
            connectedEvent.Reset();
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

                        if(IsConnected)
                        {
                            connectedEvent.Set();
                        }
                        else if (LostConnectionModal.Instance)
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
        
        public void Send(Packet packet)
        {
            byte[] bytes = packet.Serialize();

            NetOutgoingMessage om = client.CreateMessage();
            om.Write(bytes);

            client.SendMessage(om, packet.DeliveryMethod, (int)packet.UdpChannel);
            client.FlushSendQueue();
        }

        public void Stop()
        {
            client.Disconnect("");
        }
    }
}
