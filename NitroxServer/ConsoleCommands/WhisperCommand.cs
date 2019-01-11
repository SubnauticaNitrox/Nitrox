﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.Packets;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;

namespace NitroxServer.ConsoleCommands
{
    class WhisperCommand : Command
    {
        private readonly PlayerManager playerManager;

        public WhisperCommand(PlayerManager playerManager) : base("w", Perms.Player, Optional<string>.Of("w <PlayerName> <msg>"))
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            Player foundPlayer;
            if (playerManager.TryGetPlayerByName(args[0], out foundPlayer))
            {
                args = args.Skip(1).ToArray();
                if (player.IsPresent())
                {
                    foundPlayer.SendPacket(new ChatMessage(player.Get().Id, string.Join(" ", args)));
                }
                else
                {
                    foundPlayer.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, string.Join(" ", args)));
                }
            }
            else
            {
                if (player.IsPresent())
                {
                    player.Get().SendPacket(new ChatMessage(ChatMessage.SERVER_ID, "Player not found!"));
                }
                else
                {
                    Log.Info("Player not found!");
                }
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            Player player;
            return playerManager.TryGetPlayerByName(args[0], out player);
        }
    }
}
