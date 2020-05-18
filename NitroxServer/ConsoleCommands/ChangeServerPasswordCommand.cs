using System;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class ChangeServerPasswordCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public ChangeServerPasswordCommand(ServerConfig serverConfig) : base("changeserverpassword", Perms.ADMIN, "Changes server password. Clear it without argument")
        {
            this.serverConfig = serverConfig;
            AddParameter(new TypeString("password", false));
        }

        protected override void Execute(CallArgs args)
        {
            try
            {
                string playerName = args.Sender.HasValue ? args.Sender.Value.Name : "SERVER";
                string password = args.Args.Length == 0 ? string.Empty : args.Args[0];
                serverConfig.ServerPassword = password;

                Log.InfoSensitive("Server password changed to {password} by {playername}", password, playerName);
                SendMessageToPlayer(args.Sender, "Server password changed");
            }
            catch (Exception ex)
            {
                string password = args.Args.Length == 0 ? string.Empty : args.Args[0];
                Log.ErrorSensitive(ex, "Error attempting to change server password to {password}", password);
            }
        }
    }
}
