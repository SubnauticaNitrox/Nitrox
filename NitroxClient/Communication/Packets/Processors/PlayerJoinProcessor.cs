using System.Reflection;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayerJoinProcessor : ClientPacketProcessor<PlayerJoin>
    {
        private readonly PlayerManager remotePlayerManager;

        public PlayerJoinProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(PlayerJoin joinPacket)
        {
            FieldInfo field = typeof(PingManager).GetField("colorOptions", BindingFlags.Static | BindingFlags.Public);
            Color[] colors = PingManager.colorOptions;

            Color[] colorOptions = new Color[]
            {
                joinPacket.PlayerColor
            };

            //Replace the normal colorOptions with our colorOptions (has one color more iwth the player-color). Set the color of the ping with this Replace it back.
            field.SetValue(null, colorOptions);
            remotePlayerManager.FindOrCreate(joinPacket.PlayerId).Ping.SetColor(0);
            field.SetValue(null, colors);
        }
    }
}
