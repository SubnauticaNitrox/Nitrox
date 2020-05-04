using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class LoginCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public LoginCommand(ServerConfig serverConfig) : base("login", Perms.PLAYER, "Log in to server as admin (requires password)")
        {
            this.serverConfig = serverConfig;
            AddParameter(new TypeString("password", true));
        }

        protected override void Execute(CallArgs args)
        {
            Validate.IsTrue(args.Sender.HasValue, "This command can't be used by CONSOLE");

            if (args.Get(0) == serverConfig.AdminPassword)
            {
                args.Sender.Value.Permissions = Perms.ADMIN;
                SendMessage(args.Sender, $"Updated permissions to admin for {args.SenderName}");
            }
            else
            {
                SendMessage(args.Sender, "Incorrect Password");
            }
        }
    }
}
