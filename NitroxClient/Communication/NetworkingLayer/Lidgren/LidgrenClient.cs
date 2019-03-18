using System;
using System.Collections.Generic;
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

        private long incomingSequence = 0;
        private long outgoingSequence = 0;
        private int packetSentCacheCount = 1000;
        private List<Packet> packetSentCache = new List<Packet>();
        private List<Packet> packetPreReceivedCache = new List<Packet>();
        private Packet latestSkippedPackageCausedOfResendRequests = null;

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
            config.SendBufferSize = 1048576;
            client = new NetClient(config);
            client.RegisterReceivedCallback(ReceivedMessage);
            client.Start();
            client.Connect(ipAddress, serverPort);
            connectedEvent.WaitOne(2000);
            connectedEvent.Reset();
        }

        public void Send(Packet packet)
        {
            outgoingSequence++;
            packet.Sequence = outgoingSequence;
            packetSentCache.Add(packet);
            if (packetSentCache.Count > packetSentCacheCount)
            {
                packetSentCache.RemoveAt(0);
            }
#if TRACE && PACKET
            NitroxModel.Logger.Log.Info("Connection: Sending new prepared Package: " + outgoingSequence + ' ' + packet.GetType().ToString());
#endif
            SendPreparedPacket(packet);
        }

        private void ResendPackage(long sequence)
        {
            if (packetSentCache.Exists(packet => packet.Sequence == sequence))
            {
                Packet packetToResend = packetSentCache.Find(packet => packet.Sequence == sequence);
#if TRACE && PACKET
                NitroxModel.Logger.Log.Info("Connection: Resending Package: " + sequence + ' ' + packetToResend.GetType().ToString());
#endif
                SendPreparedPacket(packetToResend);
            }
            else
            {
#if TRACE && PACKET
                NitroxModel.Logger.Log.Info("Connection: Sending Package not in Cache notification: " + sequence );
#endif
                SendPreparedPacket(new PacketResendRequest(sequence * -1));
            }
        }

        private void RequestResendPackage(long sequence)
        {
#if TRACE && PACKET
            NitroxModel.Logger.Log.Info("Connection: Sending Resend request: " + sequence);
#endif
            SendPreparedPacket(new PacketResendRequest(sequence));
        }

        private void SendPreparedPacket(Packet packet)
        {
            byte[] bytes = packet.Serialize();
#if TRACE && PACKET
            NitroxModel.Logger.Log.Info("Connection: Sending prepared package: Method: " + packet.DeliveryMethod + " Channel: " + (int)packet.UdpChannel);
#endif
            NetOutgoingMessage om = client.CreateMessage();
            om.Write(bytes);            
#if TRACE && PACKET
            NitroxModel.Logger.Log.Info("Connection: Message Data Length: " + om.Data.Length);
#endif
            NetSendResult result = client.SendMessage(om, NitroxDeliveryMethod.ToLidgren(packet.DeliveryMethod), (int)packet.UdpChannel);
#if TRACE && PACKET
            NitroxModel.Logger.Log.Info("Connection: Message Send Result: " + result);
#endif
        }

        public void Stop()
        {
            client.UnregisterReceivedCallback(ReceivedMessage);
            client.Disconnect("");
            packetSentCache.Clear();
            packetPreReceivedCache.Clear();
            latestSkippedPackageCausedOfResendRequests = null;
            incomingSequence = 0;
            outgoingSequence = 0;
        }
    
        public void ReceivedMessage(object peer)
        {
            NetIncomingMessage im;
            while ((im = client.ReadMessage()) != null)
            {
#if TRACE && PACKET
                NitroxModel.Logger.Log.Info(DateTime.Now.Ticks + " Connection: Received MessageString: " + im.MessageType + ' ' + im.Data.Length + ' ' + im.ToString());
#endif
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
                            Packet packet = null;
                            
#if TRACE && PACKET
                            NitroxModel.Logger.Log.Info(DateTime.Now.Ticks + " Connection: Received Data: " + im.LengthBytes + " " + im.PositionInBytes + " " + im.Data);
#endif
                            byte[] data = new byte[im.LengthBytes];
                            im.ReadBytes(data, im.PositionInBytes, im.LengthBytes);                            
                            packet = Packet.Deserialize(data);

#if TRACE && PACKET
                            NitroxModel.Logger.Log.Info(DateTime.Now.Ticks + " Connection: Received Packet: " + packet.Sequence + '/' + incomingSequence + ' ' + packet.GetType().ToString());
#endif
                            if (packet is PacketResendRequest)
                            {
                                if (((PacketResendRequest)packet).Sequence >= 0)
                                {
                                    ResendPackage(((PacketResendRequest)packet).Sequence);
                                }
                                else
                                {
                                    //incoming PacketResendRequest was a notification from sender 
                                    //that the requested packet was not found in SentCache of sender
                                    long _sequenceToRemove = ((PacketResendRequest)packet).Sequence * -1;

#if TRACE && PACKET
                                    NitroxModel.Logger.Log.Info("Connection: Received info for unresendable Package: " + _sequenceToRemove + '/' + incomingSequence + ' ' + packet.GetType().ToString());
#endif

                                    if (_sequenceToRemove <= incomingSequence)
                                    {
                                        //already processed, incoming is a duplicate of an old resend request
                                    }
                                    else
                                    {
                                        //error handling from this point on in later version
                                        //for now, skip requesting resends for this sequence number > desynct state from here on 

                                        //only skip for one package if this is the next in the order, for all higher ones let it untouched
                                        if (incomingSequence + 1 == _sequenceToRemove)
                                        {
                                            incomingSequence++;
                                        }

                                        //try to go on with all other precached received packets
                                        if (latestSkippedPackageCausedOfResendRequests != null)
                                        {
                                            List<Packet> packetsToProcess = SyncReceiveOrder(latestSkippedPackageCausedOfResendRequests);
                                            foreach (Packet packetToProcess in packetsToProcess)
                                            {
#if TRACE && PACKET
                                                NitroxModel.Logger.Log.Info("Processing after Skip: Packet: " + packetToProcess.Sequence + '/' + incomingSequence + ' ' + packetToProcess.GetType().ToString());
#endif
                                                packetReceiver.PacketReceived(packetToProcess);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {


                                List<Packet> packetsToProcess = SyncReceiveOrder(packet);

                                foreach (Packet packetToProcess in packetsToProcess)
                                {
#if TRACE && PACKET
                                    NitroxModel.Logger.Log.Info("Processing: Packet: " + packetToProcess.Sequence + '/' + incomingSequence + ' ' + packetToProcess.GetType().ToString());
#endif
                                    packetReceiver.PacketReceived(packetToProcess);
                                }
                            }

                        }

                        break;
                    default:
                        Log.Info("Unhandled lidgren message type: " + im.MessageType + " " + im.LengthBytes + " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);
                        break;
                }

                client.Recycle(im);
            }
        }

        private List<Packet> SyncReceiveOrder(Packet packet)
        {
            List<Packet> _result = new List<Packet>();

            if (packet.Sequence == incomingSequence + 1)
            {
                //case packet is in right order and next expected to be received, all previous packages have been processed

                //check if it is a resented packet and we know newer packets ahead
                if (latestSkippedPackageCausedOfResendRequests != null)
                {
                    packetPreReceivedCache.Add(packet);
                    _result = SyncReceiveOrder(latestSkippedPackageCausedOfResendRequests);
                    return _result;
                }
                else
                {
                    _result.Add(packet);
                    incomingSequence = packet.Sequence;
                    return _result;
                }
            }

            if (packet.Sequence > incomingSequence + 1)
            {

                //case packet is further than expected
                //check all precached packets if order can be restored
                List<long> _missingPackages = new List<long>();

                for (long i = incomingSequence + 1; i < packet.Sequence; i++)
                {
                    _missingPackages.Add(i);
                }

                foreach (Packet _cached in packetPreReceivedCache)
                {
                    if (_missingPackages.Contains(_cached.Sequence))
                    {
                        _missingPackages.Remove(_cached.Sequence);
                    }
                }

                if (_missingPackages.Count == 0)
                {

                    //everything is fine, we know all packages
                    for (long i = incomingSequence + 1; i < packet.Sequence; i++)
                    {
                        Packet _packageToProcess = packetPreReceivedCache.Find(e => e.Sequence == i);
                        packetPreReceivedCache.Remove(_packageToProcess);
                        _result.Add(_packageToProcess);
                    }

                    _result.Add(packet);

                    incomingSequence = packet.Sequence;

                    //check if packet is the most newest or only the end of a partial sequence
                    if (latestSkippedPackageCausedOfResendRequests != null && latestSkippedPackageCausedOfResendRequests == packet)
                    {
                        latestSkippedPackageCausedOfResendRequests = null;
                    }

                    //clean up cache
                    if (packetPreReceivedCache.Exists(e => e.Sequence < packet.Sequence))
                    {
                        packetPreReceivedCache.RemoveAll(e => e.Sequence < packet.Sequence);
                    }
                }
                else
                {
                    //case packets in pre order are missed
                    //make resend request
                    foreach (int i in _missingPackages)
                    {
                        RequestResendPackage(i);
                    }

                    //check if we already know the current package
                    if (!packetPreReceivedCache.Exists(e => e.Sequence == packet.Sequence))
                    {
                        packetPreReceivedCache.Add(packet);
                    }

                    //check if we have to mark this package as the latest, or if this one is one of the missing between
                    if (latestSkippedPackageCausedOfResendRequests != null)
                    {
                        if (packet.Sequence > latestSkippedPackageCausedOfResendRequests.Sequence)
                        {
                            latestSkippedPackageCausedOfResendRequests = packet;
                        }
                    }
                    else
                    {
                        latestSkippedPackageCausedOfResendRequests = packet;
                    }

                    //check if we can progress partially in the sequence
                    long latestSequenceInOrder = incomingSequence;

                    for (long i = incomingSequence + 1; i < packet.Sequence; i++)
                    {
                        if (packetPreReceivedCache.Exists(e => e.Sequence == i))
                        {
                            latestSequenceInOrder = i;
                        }
                        else
                        {
                            break;
                        }
                    }

                    for (long i = incomingSequence + 1; i <= latestSequenceInOrder; i++)
                    {
                        Packet _packageToProcess = packetPreReceivedCache.Find(e => e.Sequence == i);
                        packetPreReceivedCache.Remove(_packageToProcess);
                        _result.Add(_packageToProcess);
                    }

                    incomingSequence = latestSequenceInOrder;

                    if (packetPreReceivedCache.Exists(e => e.Sequence < latestSequenceInOrder))
                    {
                        packetPreReceivedCache.RemoveAll(e => e.Sequence < latestSequenceInOrder);
                    }
                    return _result;

                }

            }

            if (packet.Sequence < incomingSequence + 1)
            {
                //old duplicate that has already been processed
            }

            return _result;
        }
    }
}
