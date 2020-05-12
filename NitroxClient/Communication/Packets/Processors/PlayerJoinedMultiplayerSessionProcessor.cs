using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.Helper;

namespace NitroxClient.Communication.Packets.Processors
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
