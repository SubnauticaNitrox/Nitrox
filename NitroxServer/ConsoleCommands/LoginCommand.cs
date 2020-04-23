using NitroxModel.DataStructures.GameLogic;
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
            string message = "Can't update permissions";

            if (args.Sender.HasValue)
            {
                if (args.Get(0) == serverConfig.AdminPassword)
                {
                    args.Sender.Value.Permissions = Perms.ADMIN;
                    message = $"Updated permissions to admin for {args.Sender.Value.Name}";
                }
                else
                {
                    message = "Incorrect Password";
                }
            }

            SendMessage(args.Sender, message);
        }
    }
}
