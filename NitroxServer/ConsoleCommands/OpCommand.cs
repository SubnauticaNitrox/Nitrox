using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic.Players;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    class OpCommand : Command
    {
        private readonly PlayerData playerData;
        private readonly PlayerManager playerManager;

        public OpCommand(PlayerData playerData, PlayerManager playerManager) : base("op", "Did you really forget the players name?", new string[] {})
        {
            this.playerData = playerData;
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args)
        {
            playerData.UpdatePlayerPermissions(args[0], Perms.Admin);
        }

        public override bool VerifyArgs(string[] args)
        {
            bool playerFound = false;
            foreach(Player player in playerManager.GetPlayers())
            {
                if (player.Name == args[0])
                {
                    playerFound = true;
                    break;
                }
            }

            return playerFound;
        }
    }
}
