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

        public ListCommand(PlayerManager playerManager)
        {
            Name = "list";
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args)
        {
            if (playerManager != null)
            {
                string list = string.Empty;
                Player[] players = playerManager.GetPlayers().ToArray();
                foreach (Player player in players)
                {
                    if (string.IsNullOrEmpty(list))
                    {
                        list += "Players: " + player.Name;
                    }
                    else
                    {
                        list += ", " + player.Name;
                    }
                }
                if (!string.IsNullOrEmpty(list))
                {
                    Log.Info(list);
                }
                else
                {
                    Log.Info("No players online");
                }
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return (args.Length == 0);
        }
    }
}
