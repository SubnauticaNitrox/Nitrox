using System.Linq;
using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class LoginCommand : Command
    {
        private readonly PlayerManager playerManager;

        public LoginCommand(PlayerManager playerManager) : base("login")
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Player player)
        {
            if (args[0] == Properties.ServerSessionSettings.Default.ServerAdminPassword)
            {
                player.isAdmin = true;
                Log.Info("Admin Logged In");
            }
            else
            {
                Log.Info("Incorrect Admin Password Attempt");
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 1;
        }
    }
}

