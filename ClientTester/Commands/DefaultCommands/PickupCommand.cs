using System;
using System.Linq;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class PickupCommand : NitroxCommand
    {
        public PickupCommand()
        {
            Name = "pickup";
            Description = "Picks up an item at location.";
            Syntax = "pickup <gameobjectname> <x> <y> <z>";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            if (args.Length < 4)
            {
                CommandManager.NotEnoughArgumentsMessage(4, Syntax);
                return;
            }
            GameObject item = new GameObject();
            item.AddComponent<UniqueIdentifier>().Id = Guid.NewGuid().ToString();
            item.transform.position = CommandManager.GetVectorFromArgs(args, 1);
            client.PacketSender.PickupItem(item, args[0]);
        }
    }
}
