using System;
using System.Collections.Generic;
using System.Linq;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.Logger;
using System.Reflection;

namespace NitroxServer.ConsoleCommands.Processor
{
    public class ConsoleCommandProcessor
    {
        public static void ProcessCommand(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                return;
            }

            string[] parts = msg.Split()
                                .Where(arg => !string.IsNullOrEmpty(arg))
                                .ToArray();

            IEnumerable<Command> discoveredCommands = FindCommands();

            foreach (Command command in discoveredCommands)
            {
                if (command.Name == parts[0])
                {
                    RunCommand(command, parts);
                }
                else
                {
                    if (command.Alias != null)
                    {
                        int index = Array.IndexOf(command.Alias, parts[0]);
                        if (index != -1)
                        {
                            RunCommand(command, parts);
                        }
                    }
                }
            }
        }

        private static IEnumerable<Command> FindCommands()
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(p => typeof(Command).IsAssignableFrom(p) &&
                            p.IsClass && !p.IsAbstract
                      )
                .Select(NitroxModel.Core.NitroxServiceLocator.LocateService)
                .Cast<Command>();
        }

        private static void RunCommand(Command command, string[] parts)
        {
            string[] args = parts.Skip(1).ToArray();

            if (command.VerifyArgs(args))
            {
                command.RunCommand(args);
                return;
            }

            Log.Info("Command Invalid: {0}", command.Args);
        }
    }
}
