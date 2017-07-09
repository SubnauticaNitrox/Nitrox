using System;
using System.Linq;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class BuildCommand : NitroxCommand
    {
        public BuildCommand()
        {
            Name = "build";
            Description = "Builds an object with the builder tool.";
            Syntax = "build <techtype> <x> <y> <z> [xrot] [yrot] [zrot]";
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
                client.PacketSender.BuildItem(args[0], CommandManager.GetVectorFromArgs(args, 1), CommandManager.GetQuaternionFromArgs(args, 4));
            }
            else
            {
                client.PacketSender.BuildItem(args[0], CommandManager.GetVectorFromArgs(args, 1), Quaternion.identity);
            }
        }
    }
}
