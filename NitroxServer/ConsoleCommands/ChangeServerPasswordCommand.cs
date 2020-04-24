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
            string password = args.Get(0) ?? string.Empty;

            serverConfig.ServerPassword = password;

            Log.Info($"Server password changed to \"{password}\" by {args.SenderName}");
            SendMessageToPlayer(args.Sender, "Server password changed");
        }
    }
}
