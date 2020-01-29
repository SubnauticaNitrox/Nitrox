﻿using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Players;

namespace NitroxServer.ConsoleCommands
{
    internal class OpCommand : Command
    {
        private readonly PlayerData playerData;
        private readonly PlayerManager playerManager;

        public OpCommand(PlayerData playerData, PlayerManager playerManager) : base("op", Perms.ADMIN, "<name>", "Set an user as admin")
        {
            this.playerData = playerData;
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            string playerName = args[0];
            string message;

            if (playerData.SetPermissions(playerName, Perms.ADMIN))
            {
                message = $"Updated '{playerName}' permissions to admin";
            }
            else
            {
                message = $"Could not update permissions on unknown player '{playerName}'";
            }

            Log.Info(message);
            SendServerMessageIfPlayerIsPresent(player, message);
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 1;
        }
    }
}
