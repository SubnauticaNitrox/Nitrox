using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Helpers
{
    public class ThrottledPacketSender
    {
        private readonly Dictionary<Type, ThrottledPacket> throttledPackets = new Dictionary<Type, ThrottledPacket>();
        private readonly IPacketSender packetSender;

        public ThrottledPacketSender(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Update()
        {
            foreach (ThrottledPacket throttledPacket in throttledPackets.Values)
            {
                if (throttledPacket.WasSend || throttledPacket.SendTime > DateTime.UtcNow)
                {
                    continue;
                }

                if (packetSender.Send(throttledPacket.Packet))
                {
                    throttledPacket.WasSend = true;
                }
            }
        }

        public bool SendThrottled(Packet packet, float throttleTime = 0.1f)
        {
            Type packetType = packet.GetType();

            if (packetSender.IsPacketSuppressed(packetType))
            {
                return false;
            }

            if (throttledPackets.TryGetValue(packetType, out ThrottledPacket throttledPacket))
            {
                throttledPacket.ReplacePacket(packet, throttleTime);
                return true;
            }

            throttledPackets.Add(packetType, new ThrottledPacket(packet, throttleTime));
            packetSender.Send(packet);
            return true;
        }

        private class ThrottledPacket
        {
            public DateTime SendTime { get; private set; }
            public Packet Packet { get; private set; }
            public bool WasSend { get; set; }

            public ThrottledPacket(Packet packet, float throttleTime)
            {
                SendTime = DateTime.UtcNow.AddSeconds(throttleTime);
                Packet = packet;
            }

            public void ReplacePacket(Packet packet, float throttleTime)
            {
                Packet = packet;

                if (WasSend)
                {
                    SendTime = DateTime.UtcNow.AddSeconds(throttleTime);
                    WasSend = false;
                }
            }
        }
    }
}
