using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayerJoinedMultiplayerSessionProcessor : ClientPacketProcessor<PlayerJoinedMultiplayerSession>
    {
        private readonly PlayerManager playerManager;
        private readonly Entities entities;

        public PlayerJoinedMultiplayerSessionProcessor(PlayerManager playerManager, Entities entities)
        {
            this.playerManager = playerManager;
            this.entities = entities;
        }

        public override void Process(PlayerJoinedMultiplayerSession packet)
        {
            List<TechType> techTypes = packet.EquippedTechTypes.Select(techType => techType.ToUnity()).ToList();
            List<Pickupable> items = new List<Pickupable>();

            playerManager.Create(packet.PlayerContext);

            // Ensure that we don't block spawning if this has already been spawned previously (such as a disconnect). 
            entities.RemoveEntity(packet.PlayerContext.PlayerNitroxId);
        }
    }
}
