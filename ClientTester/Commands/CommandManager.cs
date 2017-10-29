using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ClientTester.Commands.DefaultCommands;
using UnityEngine;

namespace ClientTester.Commands
{
    public class CommandManager
    {
        private readonly List<NitroxCommand> commands = new List<NitroxCommand>();
        private readonly MultiplayerClient client;
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
            string[] commandArray = Regex.Matches(command, @"[\""].+?[\""]|[^ ]+")
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

        public static Vector3 GetVectorFromArgs(string[] args, int pos)
        {
            return new Vector3(float.Parse(args[pos]), float.Parse(args[pos + 1]), float.Parse(args[pos + 2]));
        }

        public static Quaternion GetQuaternionFromArgs(string[] args, int pos)
        {
            return Quaternion.Euler(float.Parse(args[pos]), float.Parse(args[pos + 1]), float.Parse(args[pos + 2]));
        }
    }

    [Serializable]
    public class InvalidArgumentException : Exception
    {
        public InvalidArgumentException(string message) : base(message)
        {
        }
    }

    [Serializable]
    public class NotEnoughArgumentsException : Exception
    {
        public NotEnoughArgumentsException(int argCount)
        {
            ArgCount = argCount;
        }

        public int ArgCount { get; }
    }
}
