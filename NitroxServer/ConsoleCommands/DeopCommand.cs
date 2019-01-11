﻿using System;
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
    class DeopCommand : Command
    {
        private readonly PlayerData playerData;
        private readonly PlayerManager playerManager;

        public DeopCommand(PlayerData playerData, PlayerManager playerManager) : base("deop", Perms.Admin, Optional<string>.Of("<name>"))
        {
            this.playerData = playerData;
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            playerData.UpdatePlayerPermissions(args[0], Perms.Player);
        }

        public override bool VerifyArgs(string[] args)
        {
            Player player;
            return playerManager.TryGetPlayerByName(args[0], out player);
        }
    }
}
