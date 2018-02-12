using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace ClientTester.Commands.DefaultCommands
{
    public class PickupCommand : NitroxCommand
    {
        private readonly Item itemBroadcaster = NitroxServiceLocator.LocateService<Item>();

        public PickupCommand()
        {
            Name = "pickup";
            Description = "Picks up an item at location.";
            Syntax = "pickup <guid> <x> <y> <z>";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            assertMinimumArgs(args, 4);

            itemBroadcaster.PickedUp(CommandManager.GetVectorFromArgs(args, 1), args[0], "");
        }
    }
}
