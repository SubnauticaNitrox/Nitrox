using System.Collections.Generic;
using System.Linq;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class PlayerJoinedMultiplayerSessionProcessor : ClientPacketProcessor<PlayerJoinedMultiplayerSession>
    {
        private readonly PlayerManager remotePlayerManager;

        public PlayerJoinedMultiplayerSessionProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(PlayerJoinedMultiplayerSession packet)
        {
            List<TechType> techTypes = packet.EquippedTechTypes.Select(techType => techType.ToUnity()).ToList();
            remotePlayerManager.Create(packet.PlayerContext, techTypes);
        }
    }
}
