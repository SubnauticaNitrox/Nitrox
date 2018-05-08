using NitroxClient.GameLogic;
using NitroxModel.Core;
using UnityEngine;

namespace ClientTester.Commands.DefaultCommands
{
    public class DropCommand : NitroxCommand
    {
        private readonly Item itemBroadcaster = NitroxServiceLocator.LocateService<Item>();

        public DropCommand()
        {
            Name = "drop";
            Description = "Drops an item at a location.";
            Syntax = "drop <techtype> <x> <y> <z>";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            AssertMinimumArgs(args, 4);

            itemBroadcaster.Dropped(new GameObject(), UWE.Utils.ParseEnum<TechType>(args[0]), CommandManager.GetVectorFromArgs(args, 1));
        }
    }
}
