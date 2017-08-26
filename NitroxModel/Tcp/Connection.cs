using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace NitroxModel.Tcp
{
    public class Connection : IProcessorContext
    {
        private Socket Socket;
        private MessageBuffer MessageBuffer;
        public bool Open { get; private set; }
        public bool Authenticated { get; set; }

        public Connection(Socket socket)
        {
            this.Socket = socket;
            this.MessageBuffer = new MessageBuffer();
            this.Open = true;
        }

        public void Connect(IPEndPoint remoteEP)
        {
            try
            {
                Socket.Connect(remoteEP);
                if (!Socket.Connected)
                {
                    Open = false;
                }
            }
            catch (SocketException)
            {
                Open = false;
            }
        }
        
        public void BeginReceive(AsyncCallback callback)
        {
            try
            {
                Socket.BeginReceive(MessageBuffer.ReceivingBuffer, 0, MessageBuffer.RECEIVING_BUFFER_SIZE, 0, callback, this);
            }
            catch (SocketException se)
            {
                Console.WriteLine("Error reading data into buffer: " + se.Message);
                Open = false;
            }
        }

        public IEnumerable<Packet> GetPacketsFromReceivedData(IAsyncResult ar)
        {
            int bytesRead = 0;
            try
            {
                bytesRead = Socket.EndReceive(ar);
            }
            catch (SocketException)
            {
                Console.WriteLine("Error reading data from socket");
                Open = false;
            }

            if (bytesRead > 0)
            {
                foreach(Packet packet in MessageBuffer.GetReceivedPackets(bytesRead))
                {
                    yield return packet;
                }
            }
            else
            {
                Console.WriteLine("No data found from socket, disconnecting");
                Open = false;
            }
        }

        public void SendPacket(Packet packet, AsyncCallback callback)
        {
            if (Open) // Can remove check if able to unload Mono behaviors
            {
                byte[] packetData = packet.SerializeWithHeaderData();
                try
                {
                    Socket.BeginSend(packetData, 0, packetData.Length, 0, callback, Socket);
                }
                catch (SocketException)
                {
                    Console.WriteLine("Error sending packet");
                    Open = false;
                }
            }
        }

        public void Close()
        {
            Open = false;
            try
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Error closing socket -- probably already closed");
            }
        }
    }
}
