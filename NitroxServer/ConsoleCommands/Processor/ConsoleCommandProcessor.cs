using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.Logger;
using System.Reflection;

namespace NitroxServer.ConsoleCommands.Processor
{
    public class ConsoleCommandProcessor
    {
        private static Dictionary<string, Command> commands = new Dictionary<string, Command>();

        public static void RegisterCommands()
        {
            IEnumerable<Command> discoveredCommands = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(p => typeof(Command).IsAssignableFrom(p) &&
                            p.IsClass && !p.IsAbstract
                      )
                .Select(Activator.CreateInstance)
                .Cast<Command>();

            foreach (Command cmd in discoveredCommands)
            {
                RegisterCommand(cmd);
            }
        }

        public static void RegisterCommand(Command command)
        {
            commands.Add(command.Name, command);
        }

        public static void ProcessCommand(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                return;
            }

            Command cmd;
            string[] args = msg.Split()
                .Where(arg => !string.IsNullOrEmpty(arg))
                .ToArray();

            if (commands.TryGetValue(args[0], out cmd))
            {
                if (cmd.VerifyArgs(args))
                {
                    cmd.RunCommand(args);
                    return;
                }
                Log.Info("Command Invalid: {0}", cmd.Args);
            }
        }
    }
}
