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

        public KickCommand(PlayerManager playerManager, EntitySimulation entitySimulation) : base("kick", Perms.ADMIN, "{name} [{Reason}]", "Kicks a player from the server")
        {
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            try
            {
                Player playerToKick = playerManager.GetConnectedPlayers().Single(t => t.Name == args[0]);

                playerToKick.SendPacket(new PlayerKicked($"You were kicked from the server ! \n Reason : {string.Join(" ", args.Skip(1))}"));
                playerManager.PlayerDisconnected(playerToKick.connection);
                List<SimulatedEntity> revokedEntities = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(playerToKick);

                if (revokedEntities.Count > 0)
                {
                    SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(revokedEntities);
                    playerManager.SendPacketToAllPlayers(ownershipChange);
                }

                playerManager.SendPacketToOtherPlayers(new Disconnect(playerToKick.Id), playerToKick);
                Notify(sender, $"The player {args[0]} has been disconnected");
            }
            catch (InvalidOperationException)
            {
                Notify(sender, $"Error attempting to kick: {args[0]}, Player is not found");
            }
            catch (Exception ex)
            {
                Log.Error($"Error attempting to kick: {args[0]}", ex);
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length >= 1;
        }
    }
}
