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
            int bytesRead = Socket.EndReceive(ar);

            if (bytesRead > 0)
            {
                foreach(Packet packet in MessageBuffer.GetReceivedPackets(bytesRead))
                {
                    yield return packet;
                }
            }
            else
            {
                Open = false;
            }
        }

        public void SendPacket(Packet packet, AsyncCallback callback)
        {
            byte[] packetData = packet.SerializeWithHeaderData();
            Socket.BeginSend(packetData, 0, packetData.Length, 0, callback, Socket);
        }

        public void Close()
        {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
        }
    }
}
