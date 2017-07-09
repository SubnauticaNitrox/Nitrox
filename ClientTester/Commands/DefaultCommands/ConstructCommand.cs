using System;
using System.Linq;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class ConstructCommand : NitroxCommand
    {
        public ConstructCommand()
        {
            Name = "construct";
            Description = "Sets a base item's constructed amount. Set to 1 to build.";
            Syntax = "construct <amount> <x> <y> <z>";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            Name = "construct";
            Description = "Sets a base item's constructed amount. Set to 1 to build.";
            Syntax = "construct <amount> <x> <y> <z>";
            if (args.Length < 4)
            {
                CommandManager.NotEnoughArgumentsMessage(4, Syntax);
                return;
            }

            client.PacketSender.ChangeConstructionAmount(CommandManager.GetVectorFromArgs(args, 1), float.Parse(args[0]), 0, 0);
        }
    }
}
