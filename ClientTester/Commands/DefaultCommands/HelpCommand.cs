using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class HelpCommand : NitroxCommand
    {
        List<NitroxCommand> commands;
        public HelpCommand(List<NitroxCommand> commands)
        {
            this.commands = commands;
            Name = "help";
            Description = "Shows help commands";
            Syntax = "help";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            foreach (NitroxCommand command in commands)
            {
                Console.WriteLine(command.Name);
                Console.WriteLine("Syntax: " + command.Syntax);
                Console.WriteLine();
            }
        }
    }
}