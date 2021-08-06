using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal class LoginCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public LoginCommand(ServerConfig serverConfig) : base("login", Perms.PLAYER, PermsFlag.NO_CONSOLE, "Log in to server as admin (requires password)")
        {
            AddParameter(new TypeString("password", true, "The admin password for the server"));

            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            if (args.Get<string>(0) == serverConfig.AdminPassword)
            {
                args.Sender.Value.Permissions = Perms.ADMIN;
                SendMessage(args.Sender, $"Updated permissions to ADMIN for {args.SenderName}");
            }
            else
            {
                SendMessage(args.Sender, "Incorrect Password");
            }
        }
    }
}
