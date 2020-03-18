using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.ConsoleCommands
{
    internal class ListCommand : Command
    {
        private readonly PlayerManager playerManager;

        public ListCommand(PlayerManager playerManager) : base("list", Perms.PLAYER, "", "Shows who's online")
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            List<Player> players = playerManager.GetConnectedPlayers();

            string playerList = "List Command Result: " + string.Join(", ", players);

            if(players.Count == 0)
            {
                playerList += "No Players Online";
            }

            Log.Info(playerList);
            SendMessageToPlayer(sender, playerList);
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
