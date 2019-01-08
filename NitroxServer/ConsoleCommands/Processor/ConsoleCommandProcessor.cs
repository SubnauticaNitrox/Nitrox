using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Exceptions;
using NitroxServer.GameLogic;
using NitroxModel.MultiplayerSession;

namespace NitroxServer.ConsoleCommands.Processor
{
    public class ConsoleCommandProcessor
    {
        private readonly Dictionary<string, Command> commands = new Dictionary<string, Command>();

        public ConsoleCommandProcessor(IEnumerable<Command> cmds)
        {

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

        public void ProcessCommand(string msg, Player player)
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

            RunCommand(cmd, parts, player);
        }

        private void RunCommand(Command command, string[] parts, Player player)
        {
            //Verify is an admin or attempting to login
            if (player.isAdmin || (command.Name == "login"))
            {
                string[] args = parts.Skip(1).ToArray();

                if (command.VerifyArgs(args))
                {
                    command.RunCommand(args, player);
                    return;
                } else
                {
                    Log.Info("Command Invalid: {0}", command.Args);
                    return;
                }
            } 
            Log.Info(player.Name + " attempted a command");
        }
    }
}
