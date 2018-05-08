using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace ClientTester.Commands.DefaultCommands
{
    public class ChatCommand : NitroxCommand
    {
        private readonly Chat chatBroadcaster = NitroxServiceLocator.LocateService<Chat>();

        public ChatCommand()
        {
            Name = "chat";
            Description = "Sends a chat message to other players.";
            Syntax = "chat <message>";
        }

        public override void Execute(MultiplayerClient client, string[] args)
        {
            AssertMinimumArgs(args, 1);

            if (args.Length >= 2)
            {
                chatBroadcaster.SendChatMessage(string.Join(" ", args)); //does not support double spaces!
            }
            else
            {
                chatBroadcaster.SendChatMessage(args[0]);
            }
        }
    }
}
