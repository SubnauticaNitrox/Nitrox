using System.Collections.Generic;
using Lidgren.Network;
using NitroxModel.Logger;
using NitroxModel.Networking;
using NitroxModel.Packets;

namespace NitroxServer.Communication.NetworkingLayer.Lidgren
{
    public class LidgrenConnection : NitroxConnection
    {
        private readonly NetServer server;
        private readonly NetConnection connection;
        private long incomingSequence = 0;
        private long outgoingSequence = 0;
        private int packetSentCacheCount = 1000;
        private List<Packet> packetSentCache = new List<Packet>();
        private List<Packet> packetPreReceivedCache = new List<Packet>();
        private Packet latestSkippedPackageCausedOfResendRequests = null;

        public LidgrenConnection(NetServer server, NetConnection connection)
        {
            this.server = server;
            this.connection = connection;
        }

        public void SendPacket(Packet packet)
        {
            if (connection.Status == NetConnectionStatus.Connected)
            {
                outgoingSequence++;
                packet.Sequence = outgoingSequence;
                packetSentCache.Add(packet);
                if(packetSentCache.Count > packetSentCacheCount)
                {
                    packetSentCache.RemoveAt(0);
                }
                SendPreparedPacket(packet);
            }
            else
            {
                Log.Info("Cannot send packet to a closed connection.");
            }
        }

        public List<Packet> ManageNewPacket(Packet packet)
        {
            List<Packet> _result = new List<Packet>();

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

                        //try to go on with all other precached received packages
                        if (latestSkippedPackageCausedOfResendRequests != null)
                        {
                            List<Packet> packetsToProcess = SyncReceiveOrder(latestSkippedPackageCausedOfResendRequests);
                            foreach (Packet packetToProcess in packetsToProcess)
                            {
#if TRACE && PACKET
                                NitroxModel.Logger.Log.Info("Processing after Skip: Packet: " + packetToProcess.Sequence + '/' + incomingSequence + ' ' + packetToProcess.GetType().ToString());
#endif
                                _result.Add(packetToProcess);
                            }
                        }
                    }
                }
            }
            else
            {
#if TRACE && PACKET
                NitroxModel.Logger.Log.Info("Connection: Received Packet: " + packet.Sequence + '/' + incomingSequence + ' ' + packet.GetType().ToString());
#endif

                List<Packet> packetsToProcess = SyncReceiveOrder(packet);

                foreach (Packet packetToProcess in packetsToProcess)
                {
#if TRACE && PACKET
                    NitroxModel.Logger.Log.Info("Processing: Packet: " + packetToProcess.Sequence + '/' + incomingSequence + ' ' + packetToProcess.GetType().ToString());
#endif
                    _result.Add(packetToProcess);
                }
            }

            return _result;
        }
                     

        private void ResendPackage(long sequence)
        {
            if(packetSentCache.Exists(packet => packet.Sequence == sequence))
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
                NitroxModel.Logger.Log.Info("Connection: Sending Package not in Cache notification: " + sequence);
#endif
                SendPreparedPacket(new PacketResendRequest(sequence * -1));
            }
        }

        private void RequestResendPackage(int sequence)
        {
#if TRACE && PACKET
            NitroxModel.Logger.Log.Info("Connection: Sending Resend request: " + sequence);
#endif
            SendPreparedPacket(new PacketResendRequest(sequence));
        }

        private void SendPreparedPacket(Packet packet)
        {
#if TRACE && PACKET
            NitroxModel.Logger.Log.Info("Connection: Sending new prepared Package: " + packet.Sequence + ' ' + packet.GetType().ToString());
#endif
#if TRACE && PACKET
            NitroxModel.Logger.Log.Info("Connection: Sending prepared package: Method: " + packet.DeliveryMethod + " Channel: " + (int)packet.UdpChannel);
#endif
            byte[] packetData = packet.Serialize();
            NetOutgoingMessage om = server.CreateMessage();
            om.Write(packetData);
           
#if TRACE && PACKET
            NitroxModel.Logger.Log.Info("Connection: Message Data Length: " + om.Data.Length);
#endif
            NetSendResult result = connection.SendMessage(om, NitroxDeliveryMethod.ToLidgren(packet.DeliveryMethod), (int)packet.UdpChannel);
#if TRACE && PACKET
            NitroxModel.Logger.Log.Info("Connection: Message Send Result: " + result);
#endif

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
