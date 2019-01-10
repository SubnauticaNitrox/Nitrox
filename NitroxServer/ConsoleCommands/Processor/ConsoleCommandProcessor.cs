using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Exceptions;
using NitroxServer.GameLogic;
using NitroxModel.Packets;
using NitroxModel.DataStructures.GameLogic;

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

        public void ProcessCommand(string msg)
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
                return;
            }

            RunCommand(cmd, parts);
        }

        public void ProcessPlayerCommand(string msg, Player player, Perms perms)
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
                return;
            }

            if (cmd.RequiredPermLevel > perms)
            {
                player.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, "You do not have permission for this command!"));
                return;
            }

            if (cmd.SupportsClientSide)
            {
                RunPlayerCommand(cmd, parts, player);
            }
            else
            {
                RunCommand(cmd, parts);
            }
        }

        private void RunCommand(Command command, string[] parts)
        {
            string[] args = parts.Skip(1).ToArray();

            if (command.VerifyArgs(args))
            {
                command.RunCommand(args);
                return;
            }

            Log.Info("Command Invalid: {0}", command.Args);
        }

        private void RunPlayerCommand(Command command, string[] parts, Player player)
        {
            string[] args = parts.Skip(1).ToArray();

            if (command.VerifyArgs(args))
            {
                command.RunCommand(args, player);
                return;
            }

            player.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, string.Format("Command Invalid: {0}", command.Args)));
        }
    }
}
