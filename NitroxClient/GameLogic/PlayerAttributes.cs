using NitroxClient.Communication;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class PlayerAttributes
    {
        private PacketSender packetSender;

        public PlayerAttributes(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void BroadcastPlayerStats(float oxygen, float maxOxygen, float health, float food, float water)
        {
            PlayerStats playerStats = new PlayerStats(packetSender.PlayerId, oxygen, maxOxygen, health, food, water);
            packetSender.Send(playerStats);
        }

        public void SendPlayerJoin(String text, Color color)
        {
            PlayerJoin packet = new PlayerJoin(packetSender.PlayerId, color);
            packetSender.Send(packet);
        }
    }
}
