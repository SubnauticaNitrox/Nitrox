﻿using System;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConfigParser;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeAdminPasswordCommand : Command
    {
        private readonly PlayerManager playerManager;
        private readonly ServerConfig serverConfig;

        public ChangeAdminPasswordCommand(PlayerManager playerManager, ServerConfig serverConfig) : base("changeadminpassword", Perms.ADMIN, "<password>", "Change the admin password")
        {
            this.playerManager = playerManager;
            this.serverConfig = serverConfig;
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            try
            {
                string playerName = player.IsPresent() ? player.Get().Name : "SERVER";
                ChangeAdminPassword(args[0], playerName);   
            }
            catch (Exception ex)
            {
                Log.Error("Error attempting to change admin password: " + args[0], ex);
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length >= 1;
        }

        private void ChangeAdminPassword(string password, string name)
        {
            serverConfig.ChangeAdminPassword(password);
            Log.Info($"Admin password changed to \"{password}\" by {name}");
        }
    }
}
