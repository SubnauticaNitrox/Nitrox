﻿using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class ListCommand : Command
    {
        private readonly PlayerManager playerManager;

        public ListCommand(PlayerManager playerManager) : base("list", Perms.PLAYER, "Shows who's online")
        {
            this.playerManager = playerManager;
        }

        protected override void Execute(CallArgs args)
        {
            IList<string> players = playerManager.GetConnectedPlayers().Select(player => player.Name).ToList();
            string playerList = string.Join(", ", players);

            SendMessage(args.Sender, $"List of players : {(players.Count == 0 ? "No players online" : playerList)}");
        }
    }
}
