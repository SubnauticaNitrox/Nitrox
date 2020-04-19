using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic;

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

            addParameter(TypePlayer.Get, "name", true);
            addParameter(TypeString.Get, "reason", false);
        }

        protected override void Perform(Optional<Player> sender)
        {
            string playername = getArgAt(0);

            try
            {
                Player playerToKick = readArgAt(0);

                playerToKick.SendPacket(new PlayerKicked($"You were kicked from the server ! \n Reason : {getArgOverflow()}"));
                playerManager.PlayerDisconnected(playerToKick.connection);
                List<SimulatedEntity> revokedEntities = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(playerToKick);

                if (revokedEntities.Count > 0)
                {
                    SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(revokedEntities);
                    playerManager.SendPacketToAllPlayers(ownershipChange);
                }

                playerManager.SendPacketToOtherPlayers(new Disconnect(playerToKick.Id), playerToKick);
                SendMessageToBoth(sender, $"The player {playername} has been disconnected");
            }
            catch (InvalidOperationException)
            {
                SendMessageToBoth(sender, $"Error attempting to kick: {playername}, Player is not found");
            }
            catch (Exception ex)
            {
                Log.Error($"Error attempting to kick: {playername}", ex);
            }
        }
    }
}
