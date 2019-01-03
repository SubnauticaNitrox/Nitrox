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

namespace NitroxServer.ConsoleCommands
{
    internal class KickCommand : Command
    {
        private readonly EntitySimulation entitySimulation;
        private readonly PlayerManager playerManager;

        public KickCommand(PlayerManager playerManager, EntitySimulation entitySimulation) : base("kick", Optional<string>.Of("<name>"), "Kick the lowliest of cogs")
        {
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;
        }

        public override void RunCommand(string[] args)
        {
            try
            {
                DisconnectPlayer(args);
            }
            catch (Exception ex)
            {
                Log.Error("Error attempting to kick: " + args[0], ex);
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length >= 1;
        }

        private void DisconnectPlayer(string[] args)
        {
            Player player = playerManager.GetPlayers().Single(t => t.Name == args[0]);
            args = args.Skip(1).ToArray();

            player.SendPacket(new PlayerKicked("You were kicked from the server! \n Reason: " + string.Join(" ", args))); // Notify player was kicked
            playerManager.PlayerDisconnected(player.connection); // Remove kicked player from the playerManager and 
            List<SimulatedEntity> revokedGuids = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(player); // Calculate Sim Changes

            if (revokedGuids.Count > 0)
            {
                SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(revokedGuids);
                playerManager.SendPacketToAllPlayers(ownershipChange);
            }

            playerManager.SendPacketToOtherPlayers(new Disconnect(player.Id), player);
        }
    }
}
