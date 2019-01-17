﻿using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Exceptions;
using NitroxServer.GameLogic;
using NitroxModel.Packets;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.ConsoleCommands.Processor
{
    public class ConsoleCommandProcessor
    {
        private readonly Dictionary<string, Command> commands = new Dictionary<string, Command>();
        private readonly PlayerManager playerManager;

        public ConsoleCommandProcessor(IEnumerable<Command> cmds, PlayerManager playerManager)
        {
            this.playerManager = playerManager;
            foreach (Command cmd in cmds)
            {
                if (commands.ContainsKey(cmd.Name))
                {
                    throw new DuplicateRegistrationException($"Command {cmd.Name} is registered multiple times.");
                }

                commands[cmd.Name] = cmd;

                foreach (string alias in cmd.Alias)
                {
                    if (commands.ContainsKey(alias))
                    {
                        throw new DuplicateRegistrationException($"Command {alias} is registered multiple times.");
                    }

                    commands[alias] = cmd;
                }
            }
        }

        public void ProcessCommand(string msg, Optional<Player> player, Perms perms)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                return;
            }
            
            string[] parts = msg.Split()
                                .Where(arg => !string.IsNullOrEmpty(arg))
                                .ToArray();

            Command cmd;

            if (!commands.TryGetValue(parts[0], out cmd))
            {
                string errorMessage = "Command Not Found: " + parts[0];
                Log.Info(errorMessage);

                if (player.IsPresent())
                {
                    player.Get().SendPacket(new ChatMessage(ChatMessage.SERVER_ID, errorMessage));
                }

                return;
            }

            if (perms >= cmd.RequiredPermLevel)
            {
                RunCommand(cmd, parts, player);
            }
            else
            {
                cmd.SendServerMessageIfPlayerIsPresent(player, "You do not have the required permissions for this command!");
            }
        }

        private void RunCommand(Command command, string[] parts, Optional<Player> player)
        {
            string[] args = parts.Skip(1).ToArray();

            if (command.VerifyArgs(args))
            {
                command.RunCommand(args, player);
            }
            else
            {
                string errorMessage = string.Format("Received Command Arguments for {0}: {1}", command.Name, command.ArgsDescription);
                Log.Info(errorMessage);
                command.SendServerMessageIfPlayerIsPresent(player, errorMessage);
            }
        }
    }
}
