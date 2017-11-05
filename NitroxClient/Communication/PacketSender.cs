using System;
using System.Collections.Generic;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    public class PacketSender
    {
        public bool Active { get; set; }
        public string PlayerId { get; set; }

        private readonly TcpClient client;
        private readonly HashSet<Type> suppressedPacketsTypes = new HashSet<Type>();

        public PacketSender(TcpClient client, string playerId = null)
        {
            this.client = client;
            PlayerId = playerId;
            Active = false;
        }

        public void Send(Packet packet)
        {
            if (Active && !suppressedPacketsTypes.Contains(packet.GetType()))
            {
                try
                {
                    client.Send(packet);
                }
                catch (Exception ex)
                {
                    Log.InGame($"Error sending {packet}: {ex.Message}");
                    Log.Error("Error sending packet " + packet, ex);
                }
            }
        }

        public PacketSuppression<T> Suppress<T>()
        {
            return new PacketSuppression<T>(suppressedPacketsTypes);
        }
    }
}
