using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Exceptions;

namespace NitroxServer.ConsoleCommands.Processor
{
    public class ConsoleCommandProcessor
    {
        private readonly Dictionary<string, Command> commands = new();
        private readonly char[] splitChar = { ' ' };

        public ConsoleCommandProcessor(IEnumerable<Command> cmds)
        {
            foreach (Command cmd in cmds)
            {
                if (commands.ContainsKey(cmd.Name))
                {
                    throw new DuplicateRegistrationException($"Command {cmd.Name} is registered multiple times.");
                }

                commands[cmd.Name] = cmd;

                foreach (string alias in cmd.Aliases)
                {
                    if (commands.ContainsKey(alias))
                    {
                        throw new DuplicateRegistrationException($"Command {alias} is registered multiple times.");
                    }

                    commands[alias] = cmd;
                }
            }
        }

        public void ProcessCommand(string msg, Optional<Player> sender, Perms permissions)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                return;
            }

            string[] parts = msg.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);

            if (!commands.TryGetValue(parts[0], out Command command))
            {
                Command.SendMessage(sender, $"Command not found: {parts[0]}");
                return;
            }

            if (!sender.HasValue && command.Flags.HasFlag(PermsFlag.NO_CONSOLE))
            {
                Log.Error("This command cannot be used by CONSOLE");
                return;
            }

            if (command.CanExecute(permissions))
            {
                command.TryExecute(sender, parts.Skip(1).ToArray());
            }
            else
            {
                Command.SendMessage(sender, "You do not have the required permissions for this command !");
            }
        }
    }
}
