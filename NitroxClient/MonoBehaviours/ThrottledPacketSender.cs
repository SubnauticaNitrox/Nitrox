using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class ThrottledPacketSender : MonoBehaviour
    {
        private static readonly Dictionary<Type, ThrottledPacket> throttledPackets = new Dictionary<Type, ThrottledPacket>();
        private static IPacketSender packetSender;

        public void Awake()
        {
            packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
        }

        public void Update()
        {
            foreach (ThrottledPacket throttledPacket in throttledPackets.Values)
            {
                if (throttledPacket.SendTime <= DateTime.UtcNow)
                {
                    if (packetSender.Send(throttledPacket.Packet))
                    {
                        throttledPackets.Remove(throttledPacket.Packet.GetType());
                    }
                }
            }
        }


        public static bool SendThrottled(Packet packet, float throttleTime = 0.1f)
        {
            Type packetType = packet.GetType();

            if (packetSender.IsPacketSuppressed(packetType))
            {
                return false;
            }


            if (!throttledPackets.TryGetValue(packetType, out ThrottledPacket throttledPacket))
            {
                throttledPackets.Add(packetType, throttledPacket);
                packetSender.Send(packet);
            }

            throttledPacket = new ThrottledPacket(DateTime.UtcNow.AddMilliseconds(throttleTime), packet);
            return true;
        }

        private class ThrottledPacket
        {
            public DateTime SendTime { get; set; }
            public Packet Packet { get; set; }

            public ThrottledPacket(DateTime sendTime, Packet packet)
            {
                SendTime = sendTime;
                Packet = packet;
            }
        }
    }
}
