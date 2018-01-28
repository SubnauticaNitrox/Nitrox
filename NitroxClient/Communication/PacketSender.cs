using System;
using System.Collections.Generic;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    [Obsolete("WARNING - The PacketSender is set to be deprecated in the near future. Please migrate your code to use the IPackerSender abstraction instead.")]
    public class PacketSender : IPacketSender
    {
        public bool Active { get; set; }
        public string PlayerId { get; private set; }

        private readonly TcpClient client;
        private readonly HashSet<Type> suppressedPacketsTypes = new HashSet<Type>();

        public PacketSender(TcpClient client, string playerId = null)
        {
            this.client = client;
            PlayerId = playerId;
            Active = false;
        }

        public void send(Packet packet)
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
