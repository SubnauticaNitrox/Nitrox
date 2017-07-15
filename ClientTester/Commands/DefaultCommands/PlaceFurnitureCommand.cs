using System;
using System.Linq;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class PlaceFurnitureCommand : NitroxCommand
    {
        public PlaceFurnitureCommand()
        {
            Name = "placeFurniture";
            Description = "Builds a furniture object with the builder tool.";
            Syntax = "placeFurniture <techtype> <guid> <x> <y> <z> [<xrot> <yrot> <zrot>]";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            if (args.Length < 4)
            {
                CommandManager.NotEnoughArgumentsMessage(4, Syntax);
                return;
            }
            
            if (args.Length > 4)
            {
                client.PacketSender.PlaceFurniture(args[0], args[1], CommandManager.GetVectorFromArgs(args, 2), CommandManager.GetQuaternionFromArgs(args, 5));
            }
            else
            {
                client.PacketSender.PlaceFurniture(args[0], args[1], CommandManager.GetVectorFromArgs(args, 2), Quaternion.identity);
            }
        }
    }
}
