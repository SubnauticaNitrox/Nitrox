using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NitroxModel.Tcp
{
    public class Connection
    {
        public String PlayerId { get; set; }
        private Socket Socket;
        private MessageBuffer MessageBuffer;
        public Boolean Open { get; private set; }

        public Connection(Socket socket)
        {
            this.Socket = socket;
            this.MessageBuffer = new MessageBuffer();
            this.Open = true;
        }           
        
        public void BeginReceive(AsyncCallback callback)
        {
            Socket.BeginReceive(MessageBuffer.ReceivingBuffer, 0, MessageBuffer.RECEIVING_BUFFER_SIZE, 0, callback, this);
        }

        public IEnumerable<Packet> GetPacketsFromRecievedData(IAsyncResult ar)
        {
            int bytesRead = 0;
            try
            {
                bytesRead = Socket.EndReceive(ar);
            }
            catch (SocketException se)
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
            byte[] packetData = packet.SerializeWithHeaderData();
            try
            {
                Socket.BeginSend(packetData, 0, packetData.Length, 0, callback, Socket);
            }
            catch (SocketException se)
            {
                Console.WriteLine("Error sending packet");
                Open = false;
            }
        }

        public void Close()
        {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
        }
    }
}
