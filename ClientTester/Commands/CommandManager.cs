using ClientTester;
using ClientTester.Commands.DefaultCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            RegisterCommand(new PlaceBaseCommand());
            RegisterCommand(new PlaceFurnitureCommand());
            RegisterCommand(new AniCommand());
            RegisterCommand(new ChatCommand());
            RegisterCommand(new ConstructCommand());
            RegisterCommand(new DropCommand());
            RegisterCommand(new MoveCommand());
            RegisterCommand(new MoveyCommand());
            RegisterCommand(new PickupCommand());
            RegisterCommand(new PosCommand());
            RegisterCommand(new HelpCommand(commands));
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
                selectedCommand.Execute(client, commandArray.Skip(1).ToArray());
            }
        }

        public static void NotEnoughArgumentsMessage(int argCount, string syntax)
        {
            Console.WriteLine($"This command takes {argCount} arguments: " + syntax);
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
}
