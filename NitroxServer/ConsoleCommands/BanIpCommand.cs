using System.CodeDom;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal class BanIpCommand : Command
    {
        private readonly EntitySimulation entitySimulation;
        private readonly PlayerManager playerManager;

        public BanIpCommand(PlayerManager playerManager, EntitySimulation entitySimulation) : base("ban", Perms.MODERATOR, "ban a player from the server", true)
        {
            this.playerManager = playerManager;
            this.entitySimulation = entitySimulation;

            AddParameter(new TypePlayer("name/ip", true));
        }

        protected override void Execute(CallArgs args)
        {
            System.Net.IPAddress temp;
            if (!System.Net.IPAddress.TryParse(args.Get(0), out temp))
            {
                Player playerToBan = args.Get<Player>(0);
                playerToBan.SendPacket(new PlayerKicked(args.GetTillEnd(1)));
                playerManager.PlayerDisconnected(playerToBan.connection);

                List<SimulatedEntity> revokedEntities = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(playerToBan);
                if (revokedEntities.Count > 0)
                {
                    SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(revokedEntities);
                    playerManager.SendPacketToAllPlayers(ownershipChange);
                }

                Banning.IpBanning.AddNewBan(playerToBan.connection.Endpoint.Address.ToString());
                playerManager.SendPacketToOtherPlayers(new Disconnect(playerToBan.Id), playerToBan);
                SendMessage(args.Sender, $"The player {playerToBan.Name} has been banned");
            }
            else
            {
                foreach (Player playerToBan in playerManager.GetAllPlayers())
                {
                    if (playerToBan.connection.Endpoint.Address.ToString() == temp.ToString())
                    {
                        playerToBan.SendPacket(new PlayerKicked(args.GetTillEnd(1)));
                        playerManager.PlayerDisconnected(playerToBan.connection);

                        List<SimulatedEntity> revokedEntities = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(playerToBan);
                        if (revokedEntities.Count > 0)
                        {
                            SimulationOwnershipChange ownershipChange = new SimulationOwnershipChange(revokedEntities);
                            playerManager.SendPacketToAllPlayers(ownershipChange);
                        }

                        Banning.IpBanning.AddNewBan(playerToBan.connection.Endpoint.Address.ToString());
                        playerManager.SendPacketToOtherPlayers(new Disconnect(playerToBan.Id), playerToBan);
                    }
                }

                Banning.IpBanning.AddNewBan(temp.ToString());
            }
        }
    }
}
