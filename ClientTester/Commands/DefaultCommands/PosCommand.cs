using System;
using System.Linq;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class PosCommand : NitroxCommand
    {
        public override void Execute(MultiplayerClient client, string[] args)
        {
            Name = "drop";
            Description = "Drops an item at a location.";
            Syntax = "drop <techtype> <x> <y> <z>";
            if (args.Length < 4)
            {
                CommandManager.NotEnoughArgumentsMessage(1, Syntax);
                return;
            }

            Console.WriteLine(client.clientPos.ToString());
        }
    }
}
