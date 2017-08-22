﻿using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class DropCommand : NitroxCommand
    {
        public DropCommand()
        {
            Name = "drop";
            Description = "Drops an item at a location.";
            Syntax = "drop <techtype> <x> <y> <z>";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            assertMinimumArgs(args, 4);

            client.Logic.Item.Dropped(new GameObject(), UWE.Utils.ParseEnum<TechType>(args[0]), CommandManager.GetVectorFromArgs(args, 1));
        }
    }
}
