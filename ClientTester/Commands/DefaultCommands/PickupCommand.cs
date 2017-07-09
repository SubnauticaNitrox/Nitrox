using System;
using System.Linq;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class PickupCommand : NitroxCommand
    {
        public override void Execute(MultiplayerClient client, string[] args)
        {
            Name = "pickup";
            Description = "Picks up an item at location.";
            Syntax = "pickup <gameobjectname> <x> <y> <z>";
            if (args.Length < 4)
            {
                CommandManager.NotEnoughArgumentsMessage(1, Syntax);
                return;
            }

            client.PacketSender.PickupItem(CommandManager.GetVectorFromArgs(args, 1), args[0], "");
        }
    }
}
