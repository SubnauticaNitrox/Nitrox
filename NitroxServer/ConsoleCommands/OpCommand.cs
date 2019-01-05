using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic.Players;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.ConsoleCommands
{
    class OpCommand : Command
    {
        private readonly PlayerData playerData;
        private readonly PlayerManager playerManager;

        public OpCommand(PlayerData playerData, PlayerManager playerManager) : base("op", Optional<string>.Of("<name>"))
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
