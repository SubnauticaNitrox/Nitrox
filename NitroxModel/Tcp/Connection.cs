using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;

namespace NitroxModel.Tcp
{
    public class Connection : IProcessorContext
    {
        private readonly Socket socket;
        private readonly MessageBuffer messageBuffer = new MessageBuffer();
        public bool Open { get; private set; } = true;
        public bool Authenticated { get; set; }

        public Connection(Socket socket)
        {
            this.socket = socket;
        }

        public void Connect(IPEndPoint remoteEP)
        {
            try
            {
                socket.Connect(remoteEP);
                if (!socket.Connected)
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
                socket.BeginReceive(messageBuffer.ReceivingBuffer, 0, MessageBuffer.RECEIVING_BUFFER_SIZE, 0, callback, this);
            }
            catch (SocketException se)
            {
                Log.Info("Error reading data into buffer: " + se.Message);
                Open = false;
            }
        }

        public IEnumerable<Packet> GetPacketsFromRecievedData(IAsyncResult ar)
        {
            int bytesRead = 0;
            try
            {
                bytesRead = socket.EndReceive(ar);
            }
            catch (SocketException)
            {
                Log.Info("Error reading data from socket");
                Open = false;
            }

            if (bytesRead > 0)
            {
                foreach (Packet packet in messageBuffer.GetReceivedPackets(bytesRead))
                {
                    yield return packet;
                }
            }
            else
            {
                Log.Info("No data found from socket, disconnecting");
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
                    socket.BeginSend(packetData, 0, packetData.Length, 0, callback, socket);
                }
                catch (SocketException)
                {
                    Log.Info("Error sending packet");
                    Open = false;
                }
            }
        }

        public void Close()
        {
            Open = false;
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception)
            {
                Log.Info("Error closing socket -- probably already closed");
            }
        }
    }
}
