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
            Syntax = "construct <guid> <amount> <x> <y> <z>";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            Name = "construct";
            Description = "Sets a base item's constructed amount. Set to 1 to build.";
            Syntax = "construct <guid> <amount> <x> <y> <z>";
            if (args.Length < 5)
            {
                CommandManager.NotEnoughArgumentsMessage(4, Syntax);
                return;
            }

            client.PacketSender.ChangeConstructionAmount(args[0], CommandManager.GetVectorFromArgs(args, 3), float.Parse(args[1]));
        }
    }
}
