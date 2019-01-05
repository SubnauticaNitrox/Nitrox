using System.Linq;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class ListCommand : Command
    {
        private readonly PlayerManager playerManager;

        public ListCommand(PlayerManager playerManager) : base("list")
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args)
        {
            if (playerManager.GetPlayers().Any())
            {
                Log.Info("Players: " + string.Join(", ", playerManager.GetPlayers()));
            }
            else
            {
                Log.Info("No players online");
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
