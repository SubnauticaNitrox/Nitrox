using System.Linq;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using System.Collections.Generic;
using NitroxModel.Packets;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.ConsoleCommands
{
    internal class ListCommand : Command
    {
        private readonly PlayerManager playerManager;

        public ListCommand(PlayerManager playerManager) : base("list", Perms.Player)
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            List<Player> players = playerManager.GetPlayers();
            int playerCount = players.Count;

            if (player.IsEmpty())
            {
                playerCount++;
            }

            if (playerCount > 1)
            {
                players.Remove(player.Get()); // We don't want to report about us being online now do we?

                string playerList = "Players: " + string.Join(", ", players);
                if (!player.IsEmpty())
                {
                    player.Get().SendPacket(new ChatMessage(ChatMessage.SERVER_ID, playerList));
                }
                else
                {
                    Log.Info(playerList);
                }
            }
            else
            {
                if (!player.IsEmpty())
                {
                    player.Get().SendPacket(new ChatMessage(ChatMessage.SERVER_ID, "No players online"));
                }
                else
                {
                    Log.Info("No players online");
                }
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
