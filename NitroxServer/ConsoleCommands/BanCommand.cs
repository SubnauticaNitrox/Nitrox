using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.ConsoleCommands
{
    internal class BanCommand : Command
    {
        private readonly EntitySimulation entitySimulation;
        private readonly PlayerManager playerManager;

        public BanCommand(PlayerManager playerManager, EntitySimulation entitySimulation) : base("ban", Perms.ADMIN, "Bans a player from the server", true)
        {
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;

            AddParameter(new TypePlayer("name", true));
            AddParameter(new TypeString("reason", false));
        }

        protected override void Execute(CallArgs args)
        {
            Player playerToBan = args.Get<Player>(0);
            playerToBan.IsPlayerBanned = true;

            playerToBan.SendPacket(new PlayerKicked($"You were banned from the server! \n Reason : {args.GetTillEnd(1)}"));
            playerManager.PlayerDisconnected(playerToBan.connection);

            List<SimulatedEntity> revokedEntities = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(playerToBan);
            if (revokedEntities.Count > 0)
            {
                SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(revokedEntities);
                playerManager.SendPacketToAllPlayers(ownershipChange);
            }

            playerManager.SendPacketToOtherPlayers(new Disconnect(playerToBan.Id), playerToBan);
            SendMessage(args.Sender, $"The player {playerToBan.Name} has been banned");
        }
    }
}
