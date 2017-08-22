using ClientTester.Commands.DefaultCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ClientTester.Commands
{
    public class CommandManager
    {
        List<NitroxCommand> commands = new List<NitroxCommand>();
        MultiplayerClient client;
        public CommandManager(MultiplayerClient client)
        {
            this.client = client;
            RegisterCommand(new AniCommand());
            RegisterCommand(new ChatCommand());
            RegisterCommand(new DropCommand());
            RegisterCommand(new MoveCommand());
            RegisterCommand(new MoveyCommand());
            RegisterCommand(new PickupCommand());
            RegisterCommand(new PosCommand());
            RegisterCommand(new HelpCommand(commands));
            RegisterCommand(new PlayerStatsCommand());
        }

        public void RegisterCommand(NitroxCommand command)
        {
            commands.Add(command);
        }

        public void TakeCommand(string command)
        {
            String[] commandArray = Regex.Matches(command, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToArray();
            NitroxCommand selectedCommand = commands.Where(c => c.Name.ToLower() == commandArray[0].ToLower()).FirstOrDefault();
            if (selectedCommand != null)
            {
                try
                {
                    selectedCommand.Execute(client, commandArray.Skip(1).ToArray());
                }
                catch (InvalidArgumentException e)
                {
                    Console.WriteLine(e.Message + ": " + selectedCommand.Syntax);
                }
                catch (NotEnoughArgumentsException e)
                {
                    Console.WriteLine($"This command takes {e.ArgCount} arguments: " + selectedCommand.Syntax);
                }
            }
        }

        public static Vector3 GetVectorFromArgs(String[] args, int pos)
        {
            return new Vector3(float.Parse(args[pos]), float.Parse(args[pos + 1]), float.Parse(args[pos + 2]));
        }

        public static Quaternion GetQuaternionFromArgs(String[] args, int pos)
        {
            return Quaternion.Euler(float.Parse(args[pos]), float.Parse(args[pos + 1]), float.Parse(args[pos + 2]));
        }
    }

    public class InvalidArgumentException : Exception
    {
        public InvalidArgumentException(string message) : base(message)
        {
        }
    }

    public class NotEnoughArgumentsException : Exception
    {
        public NotEnoughArgumentsException(int argCount)
        {
            ArgCount = argCount;
        }

        public int ArgCount { get; }
    }
}
