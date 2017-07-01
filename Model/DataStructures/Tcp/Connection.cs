using NitroxModel.DataStructures.Tcp;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NitroxModel.DataStructures.Tcp
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
            Socket.BeginReceive(MessageBuffer.ReceivingBuffer, 0, MessageBuffer.ReceivingBufferSize, 0, callback, this);
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
            byte[] packetData = SerializePacketDataWithHeader(packet);
            Socket.BeginSend(packetData, 0, packetData.Length, 0, callback, Socket);
        }

        private byte[] SerializePacketDataWithHeader(Packet packet)
        {
            byte[] packetData;
            BinaryFormatter bf = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream())
            {
                //place holder for size, will be filled in later... allows us
                //to avoid doing a byte array merge... zomg premature optimization
                ms.Write(new Byte[] { 0x00, 0x00 }, 0, 2);
                bf.Serialize(ms, packet);
                packetData = ms.ToArray();
            }

            Int16 packetSize = (Int16)(packetData.Length - 2); // subtract 2 because we dont want to take into account the added bytes
            byte[] packetSizeBytes = BitConverter.GetBytes(packetSize);

            //premature optimization continued :)
            packetData[0] = packetSizeBytes[0];
            packetData[1] = packetSizeBytes[1];

            return packetData;
        }

        public void Close()
        {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
        }
    }
}
