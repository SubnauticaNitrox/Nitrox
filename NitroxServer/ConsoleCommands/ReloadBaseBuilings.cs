using System;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConfigParser;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.ConsoleCommands
{
    internal class LoadBase : Command
    {
        private readonly ServerConfig serverConfig;

        public LoadBase(ServerConfig serverConfig) : base("loadbase", Perms.PLAYER, "", "Re-loads all of the base items")
        {
            this.serverConfig = serverConfig;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            try
            {
                Log.Info($"Base Reloaded");
                SendMessageToPlayer(sender, "Base Reloaded");
            }
            catch (Exception ex)
            {
                Log.Error($"Error attempting to reload the base", ex);
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length >= 0;
        }
    }
}
