using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Helpers
{
    public class ThrottledPacketSender
    {
        private readonly Dictionary<object, ThrottledPacket> throttledPackets = new();
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

                if (packetSender.SendIfGameCode(throttledPacket.Packet))
                {
                    throttledPacket.WasSend = true;
                }
            }
        }

        public bool SendThrottled(Packet packet, Func<Packet, object> dedupeMethod, float throttleTime = 0.2f)
        {
            Type packetType = packet.GetType();

            if (packetSender.IsPacketSuppressed(packetType))
            {
                return false;
            }

            object dedupeKey = dedupeMethod.Invoke(packet);

            if (throttledPackets.TryGetValue(dedupeKey, out ThrottledPacket throttledPacket))
            {
                throttledPacket.ReplacePacket(packet, throttleTime);
                return true;
            }

            if (!packetSender.SendIfGameCode(packet))
            {
                return false;
            }

            throttledPackets.Add(dedupeKey, new ThrottledPacket(packet, throttleTime));
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
