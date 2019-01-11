using System.Linq;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using System.Collections.Generic;
using NitroxModel.Packets;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class ListCommand : Command
    {
        private readonly PlayerManager playerManager;

        public ListCommand(PlayerManager playerManager) : base("list", Perms.Player)
        {
            this.playerManager = playerManager;
            SupportsClientSide = true;
        }

        public override void RunCommand(string[] args, Player player)
        {
            List<Player> players = playerManager.GetPlayers();
            int playerCount = players.Count;

            if (player.Id == ChatMessage.SERVER_ID)
            {
                playerCount++;
            }

            if (playerCount > 1)
            {
                players.Remove(player); // We don't want to report about us being online now do we?

                string playerList = "Players: " + string.Join(", ", players);
                player.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, playerList));
            }
            else
            {
                player.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, "No players online"));
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
