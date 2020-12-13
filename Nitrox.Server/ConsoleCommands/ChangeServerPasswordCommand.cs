using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Logger;
using Nitrox.Server.ConsoleCommands.Abstract;
using Nitrox.Server.ConsoleCommands.Abstract.Type;
using Nitrox.Server.Serialization;

namespace Nitrox.Server.ConsoleCommands
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
            string password = args.Get(0) ?? string.Empty;

            serverConfig.ServerPassword = password;

            Log.InfoSensitive("Server password changed to {password} by {playername}", password, args.SenderName);
            SendMessageToPlayer(args.Sender, "Server password changed");
        }
    }
}
