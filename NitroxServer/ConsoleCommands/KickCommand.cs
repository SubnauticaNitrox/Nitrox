using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class KickCommand : Command
    {
        private readonly EntitySimulation entitySimulation;
        private readonly PlayerManager playerManager;

        public KickCommand(PlayerManager playerManager, EntitySimulation entitySimulation) : base("kick", Perms.ADMIN, "Kicks a player from the server", true)
        {
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;

            AddParameter(new TypePlayer("name", true));
            AddParameter(new TypeString("reason", false));
        }

        protected override void Execute(Optional<Player> sender)
        {
            string playerName = ReadArgAt(0);
            Player playerToKick = ReadArgAt<Player>(0);

            playerToKick.SendPacket(new PlayerKicked($"You were kicked from the server ! \n Reason : {GetArgOverflow()}"));
            playerManager.PlayerDisconnected(playerToKick.connection);

            List<SimulatedEntity> revokedEntities = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(playerToKick);
            if (revokedEntities.Count > 0)
            {
                SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(revokedEntities);
                playerManager.SendPacketToAllPlayers(ownershipChange);
            }

            playerManager.SendPacketToOtherPlayers(new Disconnect(playerToKick.Id), playerToKick);
            SendMessage(sender, $"The player {playerName} has been disconnected");
        }
    }
}
