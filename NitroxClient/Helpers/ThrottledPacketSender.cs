using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Helpers;

/// <summary>
///     Throttles packets by deduplicating them by packet type and sending the most up-to-date packet in an <see cref="Update" /> cycle.
/// </summary>
public class ThrottledPacketSender
{
    private readonly IPacketSender packetSender;
    private readonly Dictionary<object, ThrottledPacket> throttlePerPacketType = new();

    public ThrottledPacketSender(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    public void Update()
    {
        foreach (ThrottledPacket throttledPacket in throttlePerPacketType.Values)
        {
            if (throttledPacket.WasSend || throttledPacket.SendTime > DateTime.UtcNow)
            {
                continue;
            }

            if (!packetSender.SendIfGameCode(throttledPacket.Packet))
            {
                packetSender.Send(throttledPacket.Packet);
                throttledPacket.WasSend = true;
            }
        }
    }

    /// <summary>
    ///     Queues a packet for sending in the next <see cref="Update" /> cycle. If packet is currently suppressed, it won't be queued.
    /// </summary>
    public void SendThrottled(Packet packet, Func<Packet, object> dedupeMethod, float throttleTime = 0.2f)
    {
        Type packetType = packet.GetType();
        if (packetSender.IsPacketSuppressed(packetType))
        {
            return;
        }

        object dedupeKey = dedupeMethod(packet);
        if (!throttlePerPacketType.TryGetValue(dedupeKey, out ThrottledPacket throttledPacket))
        {
            throttlePerPacketType.Add(dedupeKey, new ThrottledPacket(packet, throttleTime));
        }
        else
        {
            throttledPacket.ReplacePacket(packet, throttleTime);
        }
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
