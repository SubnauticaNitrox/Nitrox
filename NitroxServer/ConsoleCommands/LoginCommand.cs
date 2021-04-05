using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    internal class LoginCommand : Command
    {
        private readonly ServerConfig serverConfig;

        public LoginCommand(ServerConfig serverConfig) : base("login", Perms.PLAYER, "Log in to server as admin (requires password)")
        {
            AddParameter(new TypeString("password", true));
            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            Validate.IsTrue(args.Sender.HasValue, "This command can't be used by CONSOLE");

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
