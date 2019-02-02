﻿using System;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConfigParser;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeServerPasswordCommand : Command
    {
        private readonly PlayerManager playerManager;
        private readonly ServerConfig serverConfig;

        public ChangeServerPasswordCommand(PlayerManager playerManager, ServerConfig serverConfig) : base("changeserverpassword", Perms.ADMIN, "[<password>]", "Change the server password. Clear password without argument")
        {
            this.playerManager = playerManager;
            this.serverConfig = serverConfig;
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            try
            {
                string playerName = player.IsPresent() ? player.Get().Name : "SERVER";
                ChangeServerPassword(args.Length==0?"":args[0], playerName);
            }
            catch (Exception ex)
            {
                Log.Error("Error attempting to change server password: " + args[0], ex);
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length >= 0;
        }

        private void ChangeServerPassword(string password, string name)
        {
            serverConfig.ChangeServerPassword(password);
            Log.Info($"Server password changed to \"{password}\" by {name}");
        }
    }
}
