using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using System.Reflection;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayerJoinProcessor : ClientPacketProcessor<PlayerJoin>
    {
        private PlayerManager remotePlayerManager;

        public PlayerJoinProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(PlayerJoin JoinPacket)
        {
            FieldInfo field = typeof(PingManager).GetField("colorOptions", BindingFlags.Static | BindingFlags.Public);
            Color[] colors = PingManager.colorOptions;

            for (int i = 0; i < colors.Length; i++)
            {
                colorOptions[i] = colors[i];
            }

            colorOptions[colorOptions.Length - 1] = JoinPacket.PlayerColor;
            field.SetValue(typeof(PingManager), colorOptions);
            remotePlayerManager.FindOrCreate(JoinPacket.PlayerId).Ping.SetColor(colorOptions.Length - 1);
            field.SetValue(typeof(PingManager), colors);

        }

        public Color[] colorOptions = new Color[]
        {
            new Color32(73, 190, byte.MaxValue, byte.MaxValue),
            new Color32(byte.MaxValue, 146, 71, byte.MaxValue),
            new Color32(219, 95, 64, byte.MaxValue),
            new Color32(93, 205, 200, byte.MaxValue),
            new Color32(byte.MaxValue, 209, 0, byte.MaxValue),
            new Color()
        };
    }
}
