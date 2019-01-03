using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.Logger;

namespace NitroxServer.ConsoleCommands
{
    class ListCommand : Command
    {
        private PlayerManager playerManager;

        public ListCommand(PlayerManager playerManager) : base("list")
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args)
        {
            if (playerManager == null)
            {
                Log.Debug("No player manager found!");
                return;
            }

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
            return (args.Length == 0);
        }
    }
}
